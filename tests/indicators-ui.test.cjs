const {chromium}=require(process.env.ASTERION_PLAYWRIGHT_MODULE||'playwright');
const fs=require('node:fs'),assert=require('node:assert/strict');
(async()=>{
 const cfg=JSON.parse(fs.readFileSync(process.env.ASTERION_TEST_CONFIG,'utf8').replace(/^\uFEFF/,'')),base='http://127.0.0.1:'+cfg.port;
 assert.equal((await(await fetch(base+'/state')).json()).simulation,true,'Dedicated simulation required');
 const ws=new WebSocket(base.replace('http:','ws:')+'/events'),pending=new Map();
 ws.onmessage=e=>{const m=JSON.parse(e.data);if(m.type==='ack'&&pending.has(m.requestId)){pending.get(m.requestId)(m);pending.delete(m.requestId);}};
 await new Promise((resolve,reject)=>{ws.onopen=resolve;ws.onerror=reject;});
 async function send(type,id=''){const requestId=crypto.randomUUID();const m=await new Promise((resolve,reject)=>{const timer=setTimeout(()=>reject(Error('Ack timeout')),5000);pending.set(requestId,m=>{clearTimeout(timer);resolve(m);});ws.send(JSON.stringify({type,id,requestId}));});assert.equal(m.ok,true);}
 const browser=await chromium.launch({headless:true,...(process.env.ASTERION_BROWSER_PATH?{executablePath:process.env.ASTERION_BROWSER_PATH}:{})});
 try{
  await send('context','FLIGHT');await send('resetIndicators');
  for(const [id,value] of [['doors',false],['portlocks',true],['lights',false]])await send('indicator',JSON.stringify({id,value}));
  const page=await browser.newPage({viewport:{width:2560,height:720}}),errors=[];page.on('pageerror',e=>errors.push(e.message));
  await page.goto(base);await page.getByText('CONNECTÉ',{exact:true}).waitFor();
  await page.locator('[data-action=doors].indicator-off').waitFor();await page.locator('[data-action=doors]').click();await page.waitForTimeout(220);
  await page.locator('[data-action=doors].indicator-on').waitFor();assert.match(await page.locator('[data-action=doors] .indicator-text').textContent(),/OUVERTES ≈/);
  await page.locator('[data-action=lights]').click();await page.waitForTimeout(220);await page.locator('[data-action=lights].indicator-on').waitFor();
  await page.locator('[data-action=ping]').click();await page.waitForTimeout(220);await page.getByText('Simulation : Ping',{exact:true}).waitFor();assert(await page.locator('[data-action=doors].indicator-on').count());
  await page.locator('[data-action=portlocks]').click();await page.waitForTimeout(220);await page.locator('[data-action=portlocks].indicator-off').waitFor();
  await page.locator('[data-action=engines]').click();await page.waitForTimeout(220);await page.locator('[data-action=engines].indicator-unknown').waitFor();assert.match(await page.locator('[data-action=engines] .indicator-text').textContent(),/ORDRE/);
  await page.getByRole('button',{name:'Réglages',exact:true}).click();await page.waitForTimeout(220);await page.getByRole('button',{name:'ÉTATS & VOYANTS',exact:true}).click();await page.waitForTimeout(220);
  await page.locator('[data-indicator=doorlocks] select').selectOption('on');await page.getByText(/État synchronisé/).waitFor();
  await page.reload();await page.getByText('CONNECTÉ',{exact:true}).waitFor();
  await page.locator('[data-action=doors].indicator-on').waitFor();await page.locator('[data-action=doorlocks].indicator-on').waitFor();await page.locator('[data-action=lights].indicator-on').waitFor();
  if(process.env.ASTERION_SCREENSHOT)await page.screenshot({path:process.env.ASTERION_SCREENSHOT});
  assert.deepEqual(errors,[]);
  console.log('PASS: independent persistent indicators, unknown baseline, manual synchronization, page changes and reconnect');
 }finally{ws.close();await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});

