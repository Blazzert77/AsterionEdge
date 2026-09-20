const {chromium}=require(process.env.ASTERION_PLAYWRIGHT_MODULE||'playwright');
const fs=require('node:fs'),assert=require('node:assert/strict');
(async()=>{
 const cfg=JSON.parse(fs.readFileSync(process.env.ASTERION_TEST_CONFIG));
 const base='http://127.0.0.1:'+cfg.port;
 const state=await(await fetch(base+'/state')).json();assert.equal(state.simulation,true,'Use a dedicated simulation instance');
 const browser=await chromium.launch({headless:true});
 try{
  const page=await browser.newPage({viewport:{width:2536,height:696}}),errors=[];page.on('pageerror',e=>errors.push(e.message));
  await page.goto(base);await page.getByText('CONNECTÉ',{exact:true}).first().waitFor();
  assert.equal(await page.locator('#versionBadge').innerText(),'v0.3.0 · DEV.3');
  await page.getByRole('button',{name:'VOL',exact:true}).click();
  await page.getByRole('heading',{name:'MFD PRINCIPAL',exact:true}).waitFor();
  assert((await page.locator('.mfd-action').count())>=20);
  assert.equal(await page.locator('.shield-action').count(),7);
  await page.getByRole('button',{name:'SYSTÈMES',exact:true}).click();
  const eject=page.getByRole('button',{name:/Éjection/}).first();
  await eject.click();await page.waitForTimeout(220);
  assert(!(await page.locator('#toast').innerText()).includes('Éjection'));
  const box=await eject.boundingBox();await page.mouse.move(box.x+20,box.y+20);await page.mouse.down();await page.waitForTimeout(2300);await page.mouse.up();
  await page.getByText(/Simulation : Éjection/).waitFor();
  await page.getByRole('button',{name:'À PIED',exact:true}).click();
  await page.getByRole('heading',{name:'MODE À PIED',exact:true}).waitFor();
  await page.getByRole('button',{name:'RÉGLAGES',exact:true}).click();
  await page.getByRole('heading',{name:'PERSONNALISATION',exact:true}).waitFor();
  assert.equal(await page.locator('input[type=color]').count(),4);
  assert.deepEqual(errors,[]);
  console.log('PASS: dev.3 MFD layout, shield strip, guarded danger actions, contextual foot page');
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
