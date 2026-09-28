const {chromium}=require(process.env.ASTERION_PLAYWRIGHT_MODULE||'playwright');
const fs=require('node:fs'),assert=require('node:assert/strict');
(async()=>{
 const cfg=JSON.parse(fs.readFileSync(process.env.ASTERION_TEST_CONFIG,'utf8').replace(/^\uFEFF/,''));
 const base='http://127.0.0.1:'+cfg.port;
 assert.equal((await(await fetch(base+'/state')).json()).simulation,true,'Dedicated simulation required');
 const ws=new WebSocket(base.replace('http:','ws:')+'/events'), pending=new Map();
 ws.onmessage=e=>{const m=JSON.parse(e.data);if(m.type==='ack'&&pending.has(m.requestId)){pending.get(m.requestId)(m);pending.delete(m.requestId);}};
 await new Promise((resolve,reject)=>{ws.onopen=resolve;ws.onerror=reject;});
 async function send(type,id=''){const requestId=crypto.randomUUID();const m=await new Promise((resolve,reject)=>{const timeout=setTimeout(()=>reject(Error('Ack timeout')),5000);pending.set(requestId,m=>{clearTimeout(timeout);resolve(m);});ws.send(JSON.stringify({type,id,requestId}));});return m;}
 const browser=await chromium.launch({headless:true});
 try{
  const page=await browser.newPage({viewport:{width:2560,height:720}}),errors=[];page.on('pageerror',e=>errors.push(e.message));
  await send('context','FLIGHT');await send('theme','nebula');await send('fontScale','100');await send('density','comfortable');await send('animations','1');
  await page.goto(base);await page.getByText('CONNECTÉ',{exact:true}).waitFor();
  assert.equal(await page.locator('#versionBadge').textContent(),'v0.3.2');
  assert.match(await page.locator('#clock').textContent(),/^\d\d:\d\d$/);
  // A feed event really changes the render key while the guarded pointer is held.
  const eject=page.locator('[data-action=eject]');await eject.evaluate(el=>{window.testHeldButton=el;});
  const box=await eject.boundingBox();await page.mouse.move(box.x+15,box.y+15);await page.mouse.down();await page.waitForTimeout(400);
  assert.equal((await send('test')).ok,true);await page.waitForTimeout(200);
  assert(await page.evaluate(()=>window.testHeldButton===document.querySelector('[data-action=eject]')&&window.testHeldButton.classList.contains('holding')));
  await page.waitForTimeout(1600);await page.mouse.up();await page.getByText('Simulation : Éjection',{exact:true}).waitFor();
  const size=()=>page.locator('.flight-panel [data-action=gear] b').evaluate(el=>parseFloat(getComputedStyle(el).fontSize));
  const before=await size();await send('fontScale','120');await page.waitForTimeout(150);assert((await size())>before*1.15);await send('fontScale','100');
  // Keep the stable protocol's invalid-context rejection.
  assert.equal((await send('context','123')).ok,false);assert.equal((await send('context','NOT_A_CONTEXT')).ok,false);
  if(process.env.ASTERION_SCREENSHOT)await page.screenshot({path:process.env.ASTERION_SCREENSHOT});
  await page.getByRole('button',{name:'Réglages',exact:true}).click();
  for(const name of ['COMBAT','TOUS LES SYSTÈMES']){
   await page.getByRole('button',{name,exact:true}).click();await page.locator('.group-grid').waitFor();
   assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
   assert(await page.locator('.command-card').count()>10);
   await page.getByRole('button',{name:'Réglages',exact:true}).click();
  }
  await page.getByRole('button',{name:'COMMANDES & RACCOURCIS',exact:true}).click();
  await page.getByRole('textbox',{name:'Rechercher une commande'}).fill('Démarrage');
  const row=page.locator('[data-binding=startup]');await row.locator('input[type=text]').fill('kb1_lalt+r');await row.getByRole('button',{name:'ENREGISTRER'}).click();
  await page.getByText('Raccourci enregistré : Démarrage du vaisseau',{exact:true}).waitFor();
  assert.equal((await(await fetch(base+'/state')).json()).actions.find(a=>a.id==='startup').input,'kb1_lalt+r');
  assert.deepEqual(errors,[]);console.log('PASS: state update during hold, immediate clock, actual font scaling, advanced pages, filtered binding identity, stable context validation');
 }finally{ws.close();await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
