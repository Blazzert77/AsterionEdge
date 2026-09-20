const {chromium}=require(process.env.ASTERION_PLAYWRIGHT_MODULE||'playwright');
const fs=require('node:fs'),assert=require('node:assert/strict');
(async()=>{
 const cfg=JSON.parse(fs.readFileSync(process.env.ASTERION_TEST_CONFIG,'utf8').replace(/^\uFEFF/,''));const base='http://127.0.0.1:'+cfg.port;
 assert.equal((await(await fetch(base+'/state')).json()).simulation,true,'Dedicated simulation required');
 const browser=await chromium.launch({headless:true});
 try{
  const page=await browser.newPage({viewport:{width:2560,height:720}}),errors=[];page.on('pageerror',e=>errors.push(e.message));
  await page.goto(base);await page.getByText('CONNECTÉ',{exact:true}).waitFor();
  await page.getByRole('button',{name:'VAISSEAU',exact:true}).click();await page.locator('.mfd-layout').waitFor();
  assert.equal(await page.locator('.target-panel [data-action]').count(),12);
  await page.getByRole('button',{name:'SHIELDS',exact:true}).click();assert.equal(await page.locator('.shield-action').count(),7);
  const eject=page.locator('[data-action=eject]');await eject.click();await page.waitForTimeout(250);assert(!(await page.locator('#toast').innerText()).includes('Simulation : Éjection'));
  let box=await eject.boundingBox();await page.mouse.move(box.x+15,box.y+15);await page.mouse.down();await page.waitForTimeout(2200);await page.mouse.up();await page.getByText('Simulation : Éjection',{exact:true}).waitFor();
  // Releasing a hold or losing focus must not execute another dangerous action.
  await page.waitForTimeout(250);box=await page.locator('[data-action=destruct]').boundingBox();await page.mouse.move(box.x+15,box.y+15);await page.mouse.down();await page.waitForTimeout(250);await page.evaluate(()=>window.dispatchEvent(new Event('blur')));await page.waitForTimeout(1800);await page.mouse.up();assert(!(await page.locator('#toast').innerText()).includes('Autodestruction'));
  for(const [tab,id] of [['MINE','mininglaser'],['SALVAGE','salvagelaser'],['TARGET','lock']]){await page.getByRole('button',{name:tab,exact:true}).click();await page.locator('[data-action='+id+']').waitFor();}
  await page.getByRole('button',{name:'À PIED',exact:true}).click();await page.locator('.foot-mfd').waitFor();assert.equal(await page.locator('.foot-mfd [data-action=reload]').count(),1);assert.equal(await page.locator('.emote-grid button').count(),9);
  await page.getByRole('button',{name:'Réglages',exact:true}).click();await page.getByRole('heading',{name:'PERSONNALISATION',exact:true}).waitFor();assert.equal(await page.locator('input[type=color]').count(),4);
  await page.getByRole('button',{name:'Violet',exact:true}).click();await page.waitForFunction(()=>document.documentElement.dataset.theme==='violet');
  await page.getByRole('button',{name:'COMMANDES & RACCOURCIS',exact:true}).click();
  const row=page.locator('[data-binding=startup]');await row.locator('input[type=text]').fill('kb1_lalt+n');await row.getByRole('button',{name:'ENREGISTRER'}).click();await page.getByText('Raccourci enregistré : Démarrage du vaisseau',{exact:true}).waitFor();
  assert.equal((await(await fetch(base+'/state')).json()).actions.find(a=>a.id==='startup').input,'kb1_lalt+n');
  await page.reload();await page.getByText('CONNECTÉ',{exact:true}).waitFor();assert.equal(await page.evaluate(()=>document.documentElement.dataset.theme),'violet');
  // A live idle socket must stay linked beyond the old twelve-second reconnect interval.
  await page.waitForTimeout(14000);assert.equal(await page.locator('#connection').innerText(),'CONNECTÉ');
  await page.getByRole('button',{name:'VAISSEAU',exact:true}).click();await page.locator('.mfd-layout').waitFor();
  for(const [width,height] of [[2560,720],[1920,550],[1280,400],[1024,768],[390,844]]){await page.setViewportSize({width,height});await page.waitForTimeout(100);const metrics=await page.evaluate(()=>({w:document.documentElement.clientWidth,s:document.documentElement.scrollWidth}));assert(metrics.s<=metrics.w,`Horizontal overflow at ${width}`);assert(await page.locator('.target-panel').isVisible());}
  assert.deepEqual(errors,[]);console.log('PASS: reference layouts, shields, mining/salvage, hold/cancel, foot weapons, persisted palettes and bindings, heartbeat, responsive layouts');
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1});
