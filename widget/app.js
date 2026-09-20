'use strict';
(() => {
  const $=id=>document.getElementById(id);
  const VERSION='0.3.0';
  const DEV='DEV.3';
  const pages={Home:['COCKPIT','⌂'],Combat:['COMBAT','◎'],Systems:['SYSTÈMES','⚙'],FPS:['À PIED','♙'],Settings:['RÉGLAGES','≡']};
  const actionLabels={
    master:'SCM / NAV',brake:'Frein spatial',gear:'Train',vtol:'VTOL',cruise:'Régulateur',decoupled:'Découplé',limiter:'Limiteur',gsafe:'G-Safe',esp:'ESP',landing:'Demander ATC',autoland:'Atterrissage auto',lights:'Éclairage',
    lock:'Verrouiller cible',hostile:'Hostile suivante',attacker:'Attaquant suivant',missile:'Mode missile',arm:'Armer missile',decoy:'Leurre',noise:'Noise',group:'Groupe armes',
    engines:'Moteurs',shields:'Boucliers',weapons:'Armes',coolers:'Refroidisseurs',power:'Alimentation',pw:'Priorité armes',pt:'Priorité moteurs',ps:'Priorité boucliers',balance:'Équilibrer énergie',
    friendly:'Alliée suivante',subtarget:'Sous-composant',pin:'Épingler cible',scan:'Scan',ping:'Ping',helmet:'Casque',flashlight:'Lampe',wipe:'Visière',mobiglas:'mobiGlas',inventory:'Inventaire',interact:'Interaction',heal:'Soin',
    starmap:'Carte stellaire',doors:'Portes',camera:'Caméra externe',headtrack:'Head tracking',mfd:'MFD suivant',eject:'Éjection',destruct:'Autodestruction',
    sf:'Bouclier avant',sr:'Bouclier arrière',sl:'Bouclier gauche',sright:'Bouclier droit',stop:'Bouclier haut',sbottom:'Bouclier bas',sreset:'Équilibrer boucliers',salute:'Salut',wave:'Faire signe',respect:'Respect'
  };
  const glyph={gear:'LG',vtol:'VT',landing:'ATC',master:'SCM',lights:'LGT',scan:'SCN',ping:'PNG',decoy:'CM',noise:'NS',cruise:'CRS',decoupled:'DEC',mobiglas:'MG',starmap:'MAP',lock:'TGT',hostile:'HST',attacker:'ATK',missile:'MSL',weapons:'WPN',shields:'SHD',power:'PWR',engines:'ENG',doors:'DR',camera:'CAM',inventory:'INV',flashlight:'LMP',helmet:'HLM',interact:'USE',heal:'MED',eject:'EJ',destruct:'SD'};
  const quickFlight=['gear','vtol','landing','master','lights','scan','ping','decoy','cruise','decoupled','mobiglas','starmap'];
  const quickFoot=['mobiglas','inventory','flashlight','helmet','interact','heal','starmap'];
  const systemGroups={
    'PILOTAGE':['brake','limiter','gsafe','esp','autoland','cruise','decoupled','master','gear','vtol','landing'],
    'ÉNERGIE':['power','engines','weapons','shields','coolers','pw','pt','ps','balance'],
    'BOUCLIERS':['sf','sr','sl','sright','stop','sbottom','sreset'],
    'SYSTÈMES':['lights','doors','camera','headtrack','mfd','mobiglas','starmap','scan','ping','eject','destruct']
  };
  const combatGroups={
    'CIBLAGE':['lock','hostile','attacker','friendly','subtarget','pin'],
    'ARMES':['weapons','group','missile','arm'],
    'DÉFENSE':['decoy','noise'],
    'CAPTEURS':['scan','ping']
  };
  let page='Home',state=null,ws=null,port=32147,retry=null,backoff=800,connected=false,lastContext='',hold=null,ping=null,lastMessage=0,renderKey='';
  const pending=new Map();
  $('versionBadge').textContent='v'+VERSION+' · '+DEV;
  const toast=text=>$('toast').textContent=text;
  function raw(value){if(ws?.readyState===WebSocket.OPEN)ws.send(JSON.stringify(value));}
  function send(type,id=''){
    if(!connected){toast('Companion déconnecté.');return Promise.reject(Error('Companion déconnecté'));}
    const requestId=crypto.randomUUID?crypto.randomUUID():String(Date.now()+Math.random());
    return new Promise((resolve,reject)=>{const timeout=setTimeout(()=>{pending.delete(requestId);reject(Error('Délai dépassé'));},4500);pending.set(requestId,{resolve,reject,timeout});raw({type,id,requestId});});
  }
  function disconnect(){connected=false;cancelHold();clearInterval(ping);$('connection').textContent='DÉCONNECTÉ';$('game').textContent='ARRÊTÉ';$('footerStatus').textContent='AUTO-LINK · en attente du Companion';for(const p of pending.values()){clearTimeout(p.timeout);p.reject(Error('Déconnecté'));}pending.clear();renderKey='';render();}
  function connect(){
    clearTimeout(retry);if(ws){ws.onclose=null;ws.close();}disconnect();$('connection').textContent='CONNEXION…';
    const socket=new WebSocket(`ws://127.0.0.1:${port}/events`);ws=socket;
    socket.onopen=()=>lastMessage=Date.now();
    socket.onmessage=e=>{if(socket!==ws)return;lastMessage=Date.now();let msg;try{msg=JSON.parse(e.data);}catch{return;}
      if(msg.type==='state'&&Number(msg.state?.protocol)>=2){const first=!connected;connected=true;backoff=800;state=msg.state;applyAppearance();if(first){clearInterval(ping);ping=setInterval(()=>{if(Date.now()-lastMessage>12000){ws.close();return;}raw({type:'ping'});},4000);toast('Companion connecté automatiquement.');}
        if(state.autoContext&&state.context!==lastContext){if(state.context==='ON_FOOT')page='FPS';else if(['FLIGHT','GROUND_VEHICLE','MINING','SALVAGE'].includes(state.context))page='Home';else page='Home';}
        lastContext=state.context;if(!availablePages().includes(page))page='Home';update();
      } else if(msg.type==='ack'){const p=pending.get(msg.requestId);if(p){clearTimeout(p.timeout);pending.delete(msg.requestId);msg.ok?p.resolve(msg):p.reject(Error(msg.error||'Commande refusée'));}}
    };
    socket.onerror=()=>socket.close();socket.onclose=()=>{if(socket!==ws)return;disconnect();toast('Companion indisponible · nouvelle tentative automatique.');retry=setTimeout(connect,backoff);backoff=Math.min(backoff*1.6,10000);};
  }
  function availablePages(){
    if(!state||!state.running||state.context==='UNKNOWN')return ['Home','Settings'];
    if(state.context==='ON_FOOT')return ['Home','FPS','Settings'];
    if(state.context==='GROUND_VEHICLE')return ['Home','Systems','Settings'];
    return ['Home','Combat','Systems','Settings'];
  }
  function nav(){const el=$('nav');el.replaceChildren();availablePages().forEach(p=>{const [label,g]=pages[p];const b=document.createElement('button');b.textContent=label;b.dataset.glyph=g;b.className=p===page?'active':'';b.onclick=()=>{page=p;renderKey='';render();};el.append(b);});}
  function ctxLabel(v){return({UNKNOWN:'INCONNU',ON_FOOT:'À PIED',FLIGHT:'VOL',GROUND_VEHICLE:'VÉHICULE',MINING:'MINAGE',SALVAGE:'SALVAGE'})[v]||v||'INCONNU';}
  function applyAppearance(){
    const root=document.documentElement,appearance=state?.appearance||{};
    root.dataset.theme=appearance.theme||state?.theme||'nebula';
    root.dataset.density=appearance.density||'comfortable';
    root.dataset.animations=appearance.animations===false?'off':'on';
    const color=(v,f)=>/^#[0-9a-f]{6}$/i.test(v||'')?v:f;
    root.style.setProperty('--accent',color(appearance.accent||state?.accent,'#20e0d0'));
    root.style.setProperty('--accent2',color(appearance.accent2,'#2b8cff'));
    root.style.setProperty('--bg',color(appearance.background,'#061019'));
    root.style.setProperty('--panel',color(appearance.panel,'#0d1b25'));
    root.style.setProperty('--panel-opacity',`${Math.max(60,Math.min(100,appearance.panelOpacity??92))}%`);
    root.style.setProperty('--radius',`${Math.max(0,Math.min(20,appearance.radius??8))}px`);
    root.style.setProperty('--font-scale',String(Math.max(.85,Math.min(1.2,(appearance.fontScale??100)/100))));
    root.style.setProperty('--glow-alpha',String(Math.max(0,Math.min(1,(appearance.glow??32)/100))));
    root.style.setProperty('--quick-cols',String(Math.max(6,Math.min(12,appearance.quickColumns??10))));
  }
  function update(){
    applyAppearance();$('connection').textContent='CONNECTÉ';$('game').textContent=state.running?'EN COURS':'ARRÊTÉ';$('branchBuild').textContent=state.running?`${state.branch} ${shortBuild(state.build)}`:'—';$('logState').textContent=state.logFound?'TROUVÉ':'ABSENT';$('footerStatus').textContent=state.running?`${state.branch} · ${state.build} · ${state.shard||'shard en attente'}`:'STAR CITIZEN ARRÊTÉ';render();
  }
  const shortBuild=v=>!v||v==='Unavailable'?'—':String(v).split('-')[0].slice(0,14);
  function render(){const key=JSON.stringify([page,connected,state?.running,state?.context,state?.actions,state?.feed,state?.location,state?.shard,state?.build,state?.ship,state?.appearance,state?.update]);if(key===renderKey)return;renderKey=key;cancelHold();nav();const c=$('content');c.replaceChildren();if(page==='Home')home(c);else if(page==='Combat')groupPage(c,'COMBAT & CIBLAGE','Les commandes utiles au combat, regroupées sans doublons.',combatGroups);else if(page==='Systems')groupPage(c,'SYSTÈMES DU VAISSEAU','Pilotage avancé, énergie, boucliers et systèmes secondaires.',systemGroups);else if(page==='FPS')footPage(c);else settings(c);}
  function pageHead(title,sub,context=true){const h=document.createElement('div');h.className='page-head';const left=document.createElement('div');left.innerHTML=`<div class="kicker">ASTERION / ${ctxLabel(state?.context)}</div><h1>${title}</h1><p>${sub}</p>`;h.append(left);if(context){const s=document.createElement('div');s.className='context-switch';for(const [id,label] of [['AUTO','AUTO'],['ON_FOOT','À PIED'],['FLIGHT','VOL']]){const b=document.createElement('button');b.textContent=label;b.className=contextSelected(id)?'selected':'';b.onclick=()=>send('context',id).catch(e=>toast(e.message));s.append(b);}h.append(s);}return h;}
  function contextSelected(id){if(!state)return false;if(id==='AUTO')return state.contextSource!=='MANUAL'&&state.contextSource!=='SIMULATION';return state.context===id;}
  function home(c){
    if(state?.context==='ON_FOOT'){footDashboard(c);return;}
    c.append(pageHead('MFD PRINCIPAL','Commandes de vol regroupées par fonction, avec de grandes zones tactiles.'));
    const wrap=document.createElement('section');wrap.className='mfd-layout';
    wrap.append(
      mfdPanel('POWER','ÉNERGIE & DÉMARRAGE',['power','weapons','engines','shields','coolers','pw','pt','ps','balance'],'power-panel'),
      mfdPanel('FLIGHT','PILOTAGE',['master','brake','gear','vtol','landing','autoland','decoupled','cruise','gsafe','esp','limiter'],'flight-panel'),
      mfdPanel('SHIP SYSTEMS','SYSTÈMES',['mfd','mobiglas','starmap','camera','ping','scan','lights','headtrack','doors'],'systems-panel'),
      mfdPanel('TARGET','CIBLAGE',['lock','hostile','attacker','friendly','subtarget','pin','decoy','noise','missile'],'target-panel'),
      dangerRail()
    );
    c.append(wrap,shieldStrip(),sessionBar());
  }
  function mfdPanel(title,sub,ids,extra=''){const panel=document.createElement('article');panel.className='mfd-panel '+extra;const head=document.createElement('div');head.className='mfd-head';head.innerHTML=`<div><b>${title}</b><span>${sub}</span></div>`;const grid=document.createElement('div');grid.className='mfd-grid';const actions=ids.map(id=>(state?.actions||[]).find(a=>a.id===id)).filter(Boolean);for(const a of actions)grid.append(actionButton(a,'mfd-action'));if(!actions.length){const e=document.createElement('div');e.className='empty';e.textContent='Aucune commande liée.';grid.append(e);}panel.append(head,grid);return panel;}
  function shieldStrip(){const shell=document.createElement('section');shell.className='shield-strip';const label=document.createElement('div');label.className='shield-label';label.innerHTML='<b>BOUCLIERS</b><span>Répartition — commande uniquement</span>';shell.append(label);for(const id of ['sf','sr','sl','sright','stop','sbottom','sreset']){const a=(state?.actions||[]).find(x=>x.id===id);if(a)shell.append(actionButton(a,'shield-action'));}return shell;}
  function dangerRail(){const rail=document.createElement('aside');rail.className='danger-rail';for(const id of ['eject','destruct']){const a=(state?.actions||[]).find(x=>x.id===id);if(a)rail.append(actionButton(a,'danger-tall'));}return rail;}
  function sessionBar(){const bar=document.createElement('section');bar.className='session-bar';const rows=[['VAISSEAU',state?.ship||'Non identifié'],['LOCALISATION',state?.location||'Indisponible'],['SHARD',state?.shard||'Indisponible'],['BUILD',state?.running?`${state.branch} · ${shortBuild(state.build)}`:'Hors ligne']];for(const [l,v] of rows){const el=document.createElement('div');el.innerHTML=`<span>${l}</span><b>${escapeHtml(v)}</b>`;bar.append(el);}return bar;}
  function footDashboard(c){c.append(pageHead('MODE À PIED','Interface allégée : outils utiles, emotes et journal de session.'));const wrap=document.createElement('section');wrap.className='foot-mfd';wrap.append(mfdPanel('SUIT & VIEW','PERSONNEL',['mobiglas','flashlight','inventory','helmet','wipe','camera','starmap'],'foot-panel'),mfdPanel('ACTIONS','UTILITAIRES',['interact','heal','doors'],'foot-panel'),mfdPanel('EMOTES','SOCIAL',['salute','wave','respect'],'foot-panel'),eventPanel());c.append(wrap,sessionBar());}
  function eventPanel(){const panel=document.createElement('section');panel.className='mfd-panel event-mfd';const head=document.createElement('div');head.className='mfd-head';head.innerHTML='<div><b>EVENT FEED</b><span>GAME.LOG</span></div>';panel.append(head);const list=document.createElement('div');list.className='event-list';const events=[...(state?.feed||[])].reverse().slice(0,8);if(!events.length){const e=document.createElement('div');e.className='empty';e.textContent='Aucun événement observé.';list.append(e);}else for(const e of events){const row=document.createElement('div');row.className='event-line';row.innerHTML=`<time>${escapeHtml(e.time||'--:--')}</time><span>${escapeHtml(e.text||'')}</span>`;list.append(row);}panel.append(list);return panel;}
  function quickActions(c){}
  function groupPage(c,title,sub,groups){c.append(pageHead(title,sub));const wrap=document.createElement('section');wrap.className='page-card group-grid';for(const [name,ids] of Object.entries(groups)){const g=document.createElement('div');g.className='group';const gt=document.createElement('div');gt.className='group-title';gt.textContent=name;const grid=document.createElement('div');grid.className='command-grid';const list=ids.map(id=>(state?.actions||[]).find(a=>a.id===id)).filter(Boolean);for(const a of list)grid.append(actionButton(a,'command-card'));if(!list.length){const e=document.createElement('div');e.className='empty';e.textContent='Aucune commande disponible.';grid.append(e);}g.append(gt,grid);wrap.append(g);}c.append(wrap);}
  function footPage(c){footDashboard(c);}
  function actionButton(a,cls){const b=document.createElement('button');b.className=cls+(a.dangerous?' danger':'');b.disabled=!connected||(!state?.simulation&&(!a.bound||!state?.running));const binding=state?.simulation?'SIM':a.bound?bindingLabel(a.input):a.source;if(cls==='quick-action'||cls.includes('mfd-action')||cls.includes('shield-action')||cls.includes('danger-tall')){b.innerHTML=`<span class="action-glyph">${escapeHtml(glyph[a.id]||a.id.slice(0,3).toUpperCase())}</span><b>${escapeHtml(actionLabels[a.id]||a.label)}</b><small>${a.dangerous?'MAINTENIR · ':''}${escapeHtml(binding)}</small>`;}else{b.innerHTML=`<b>${escapeHtml(actionLabels[a.id]||a.label)}</b><small>${a.dangerous?'MAINTENIR · ':''}${escapeHtml(binding)}</small>`;}if(!a.dangerous)b.onclick=()=>execute(a,b);else{const p=document.createElement('span');p.className='progress';b.append(p);b.onpointerdown=e=>startHold(e,a,b,p);b.onpointerup=b.onpointercancel=b.onlostpointercapture=cancelHold;b.oncontextmenu=e=>e.preventDefault();}return b;}
  function startHold(e,a,b,p){if(e.button!==0||b.disabled)return;e.preventDefault();cancelHold();b.setPointerCapture(e.pointerId);const h={button:b,progress:p,id:a.id,start:performance.now(),pulse:null,raf:null};hold=h;b.classList.add('holding');send('holdBegin',a.id).then(()=>{if(hold!==h)return;h.start=performance.now();h.pulse=setInterval(()=>raw({type:'holdPulse'}),150);const frame=()=>{if(hold!==h)return;const elapsed=performance.now()-h.start;p.style.width=Math.min(100,elapsed/(state?.holdDuration||1800)*100)+'%';if(elapsed>=(state?.holdDuration||1800)+80){clearInterval(h.pulse);hold=null;b.classList.remove('holding');p.style.width='0';execute(a,b);}else h.raf=requestAnimationFrame(frame);};h.raf=requestAnimationFrame(frame);}).catch(err=>{cancelHold();toast(err.message);});}
  function cancelHold(){if(!hold)return;clearInterval(hold.pulse);cancelAnimationFrame(hold.raf);hold.button.classList.remove('holding');hold.progress.style.width='0';hold=null;raw({type:'holdCancel'});}
  function execute(a,b){send('action',a.id).then(()=>{toast(state?.simulation?`Simulation : ${actionLabels[a.id]||a.label}`:`Commande envoyée : ${actionLabels[a.id]||a.label}`);b.classList.add('sent');setTimeout(()=>b.classList.remove('sent'),260);}).catch(e=>toast(e.message));}
  const bindingLabel=input=>String(input||'').replace(/^kb1_/i,'').replaceAll('_',' ').toUpperCase();
  function settings(c){c.append(pageHead('PERSONNALISATION','Réglez réellement l’apparence du cockpit : couleurs, densité, typographie, angles et effets.',false));const a=state?.appearance||{};const layout=document.createElement('section');layout.className='settings-layout';const left=document.createElement('div');left.className='settings-column';const presets=card('PRÉSETS','Choisissez une base puis ajustez chaque détail.');const pr=document.createElement('div');pr.className='preset-row';for(const [id,label] of [['nebula','Nebula'],['graphite','Graphite'],['tactical','Tactical'],['minimal','Minimal']]){const b=document.createElement('button');b.className='preset-btn '+((a.theme||'nebula')===id?'selected':'');b.textContent=label;b.onclick=()=>send('theme',id).then(()=>toast('Preset appliqué.')).catch(e=>toast(e.message));pr.append(b);}presets.append(pr);const colors=card('COULEURS','Palette complète, pas seulement une couleur d’accent.');const cg=document.createElement('div');cg.className='color-grid';for(const [key,label,fallback,type] of [['accent','Accent principal','#20e0d0','accent'],['accent2','Accent secondaire','#2b8cff','accent2'],['background','Fond','#061019','background'],['panel','Panneaux','#0d1b25','panel']]){const box=document.createElement('label');box.className='color-field';box.textContent=label;const input=document.createElement('input');input.type='color';input.value=/^#[0-9a-f]{6}$/i.test(a[key]||'')?a[key]:fallback;input.onchange=()=>send(type,input.value).then(()=>toast(`${label} enregistré.`)).catch(e=>toast(e.message));box.append(input);cg.append(box);}colors.append(cg);const controls=card('AFFICHAGE','Ajustez la lisibilité au XENEON EDGE et à votre distance de jeu.');const cl=document.createElement('div');cl.className='control-list';cl.append(slider('Opacité panneaux',a.panelOpacity??92,60,100,'%','panelOpacity'),slider('Taille du texte',a.fontScale??100,85,120,'%','fontScale'),slider('Angles',a.radius??8,0,20,'px','radius'),slider('Intensité glow',a.glow??32,0,100,'%','glow'));const density=document.createElement('div');density.className='control';density.innerHTML='<label>Densité <span></span></label>';const sel=document.createElement('select');for(const [v,l] of [['compact','Compacte'],['comfortable','Confortable'],['large','Grande']]){const o=document.createElement('option');o.value=v;o.textContent=l;o.selected=(a.density||'comfortable')===v;sel.append(o);}sel.onchange=()=>send('density',sel.value).then(()=>toast('Densité enregistrée.')).catch(e=>toast(e.message));density.append(sel);cl.append(density);const cols=slider('Actions par ligne',a.quickColumns??10,6,12,'','quickColumns');cl.append(cols);controls.append(cl);const toggles=document.createElement('div');toggles.className='toggle-row';toggles.append(toggle('Couleurs constructeur',a.manufacturerColors??state?.manufacturerColors,'manufacturerColors'),toggle('Animations',a.animations!==false,'animations'));controls.append(toggles);left.append(presets,colors,controls);const preview=card('APERÇU','Les changements sont appliqués instantanément et sauvegardés par le Companion.','preview-card');const demo=document.createElement('div');demo.className='preview-demo';demo.innerHTML='<span></span><span></span><span></span>';preview.append(demo);const vr=document.createElement('div');vr.className='version-row';vr.innerHTML=`<span>Version interface</span><strong>v${VERSION}</strong>`;preview.append(vr);const update=document.createElement('button');update.className='update-button';update.textContent=state?.update?.available?`MISE À JOUR v${state.update.latest} DISPONIBLE`:'AUCUNE MISE À JOUR PUBLIQUE';update.disabled=!state?.update?.available;update.onclick=()=>send('openUpdate').catch(e=>toast(e.message));preview.append(update);layout.append(left,preview);c.append(layout);}
  function card(title,sub,extra=''){const el=document.createElement('article');el.className='setting-card '+extra;el.innerHTML=`<h3>${title}</h3><p>${sub}</p>`;return el;}
  function slider(label,value,min,max,suffix,type){const box=document.createElement('div');box.className='control';const lab=document.createElement('label');const v=document.createElement('span');v.textContent=`${value}${suffix}`;lab.append(document.createTextNode(label),v);const input=document.createElement('input');input.type='range';input.min=min;input.max=max;input.value=value;input.oninput=()=>v.textContent=`${input.value}${suffix}`;input.onchange=()=>send(type,input.value).then(()=>toast(`${label} enregistré.`)).catch(e=>toast(e.message));box.append(lab,input);return box;}
  function toggle(label,on,type){const b=document.createElement('button');b.className='toggle '+(on?'on':'');b.textContent=`${label} : ${on?'ON':'OFF'}`;b.onclick=()=>send(type,on?'0':'1').then(()=>toast(`${label} mis à jour.`)).catch(e=>toast(e.message));return b;}
  function notice(text){const n=document.createElement('div');n.className='notice';n.textContent=text;return n;}
  function escapeHtml(v){return String(v??'').replace(/[&<>'"]/g,ch=>({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[ch]));}
  $('retryButton').onclick=()=>{backoff=800;connect();};
  window.addEventListener('blur',cancelHold);document.addEventListener('visibilitychange',()=>{if(document.hidden)cancelHold();});window.addEventListener('pagehide',()=>{cancelHold();clearTimeout(retry);clearInterval(ping);ws?.close();});window.addEventListener('resize',()=>{renderKey='';render();});
  setInterval(()=>{$('clock').textContent=new Date().toLocaleTimeString('fr-FR',{hour:'2-digit',minute:'2-digit'});},1000);
  render();connect();
})();
