const {chromium}=require(process.env.ASTERION_PLAYWRIGHT_MODULE||'playwright');
const fs=require('node:fs'),assert=require('node:assert/strict');
(async()=>{
 const cfg=JSON.parse(fs.readFileSync(process.env.ASTERION_TEST_CONFIG,'utf8').replace(/^\uFEFF/,'')),base='http://127.0.0.1:'+cfg.port;
 const state=await(await fetch(base+'/state')).json();assert.equal(state.simulation,true,'Dedicated simulation required');
 const browser=await chromium.launch({headless:true});
 try{
  const page=await browser.newPage({viewport:{width:2560,height:720}}),errors=[];page.on('pageerror',e=>errors.push(e.message));
  // Render a realistic unbound-door state without permitting any real command.
  const fixture=structuredClone(state);fixture.context='FLIGHT';fixture.contextSource='MANUAL';fixture.simulation=false;fixture.running=true;fixture.appearance.theme='nebula';fixture.appearance.accent='#00C8FA';fixture.appearance.panel='#063044';fixture.appearance.fontScale=100;fixture.appearance.radius=2;
  fixture.actions.filter(a=>['doors','doorsOpen','doorsClose'].includes(a.id)).forEach(a=>{a.bound=false;a.input='';});
  let commands=0;
  await page.routeWebSocket('**/events',ws=>{ws.send(JSON.stringify({type:'state',state:fixture}));ws.onMessage(message=>{const m=JSON.parse(message);if(m.type==='action')commands++;if(m.type==='ping')ws.send(JSON.stringify({type:'pong'}));});});
  await page.goto(base);await page.getByText('CONNECTÉ',{exact:true}).waitFor();
  for(const id of ['weaponsUp','weaponsDown','enginesUp','enginesDown','shieldsUp','shieldsDown','master','destruct'])assert(await page.locator('[data-action='+id+']').isEnabled(),id+' should resolve');
  await page.getByRole('button',{name:'POINTS',exact:true}).click();assert.equal(await page.locator('.allocation-grid').count(),3);assert.equal(await page.locator('.shield-action').count(),0);
  await page.getByRole('button',{name:'POWER',exact:true}).click();
  if(process.env.ASTERION_SCREENSHOT)await page.screenshot({path:process.env.ASTERION_SCREENSHOT});
  await page.locator('[data-action=doors]').click();await page.getByRole('heading',{name:'COMMANDES & RACCOURCIS',exact:true}).waitFor();
  assert.equal(await page.getByRole('textbox',{name:'Rechercher une commande'}).inputValue(),'portes');
  assert(await page.getByText(/déverrouiller ne les ouvre pas/).isVisible());
  for(const id of ['doors','doorsOpen','doorsClose','doorunlock','doorlock'])assert(await page.locator('[data-binding='+id+']').isVisible());
  assert.equal(commands,0,'Configuration button must not dispatch a door command');
  await page.getByRole('textbox',{name:'Rechercher une commande'}).fill('SCM');
  assert.equal(await page.locator('[data-binding=master] input[type=number]').getAttribute('min'),'400');
  assert.deepEqual(errors,[]);console.log('PASS: resolved cockpit controls, allocation tab, unbound door setup path without sending input, SCM duration guidance');
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
