'use strict';
(() => {
  const $=id=>document.getElementById(id);
  const VERSION='0.3.0';
  const DEV='DESIGN.2';
  const pages={Home:['COCKPIT','⌂'],Combat:['COMBAT','◎'],Systems:['SYSTÈMES','⚙'],FPS:['À PIED','♙'],Settings:['RÉGLAGES','≡'],Bindings:['COMMANDES','≡'],Indicators:['INDICATEURS','◉']};
  const actionLabels={
    master:'SCM / NAV',brake:'Frein spatial',gear:'Train',vtol:'VTOL',cruise:'Régulateur',decoupled:'Découplé',limiter:'Limiteur',gsafe:'G-Safe',esp:'ESP',landing:'Demander ATC',autoland:'Atterrissage auto',lights:'Éclairage',
    lock:'Verrouiller cible',hostile:'Hostile suivante',attacker:'Attaquant suivant',missile:'Mode missile',arm:'Armer missile',decoy:'Leurre',noise:'Noise',group:'Groupe armes',
    engines:'Moteurs',shields:'Boucliers',weapons:'Armes',coolers:'Refroidisseurs',power:'Alimentation',pw:'Priorité armes',pt:'Priorité moteurs',ps:'Priorité boucliers',balance:'Équilibrer énergie',
    friendly:'Alliée suivante',subtarget:'Sous-composant',pin:'Épingler cible',scan:'Scan',ping:'Ping',helmet:'Casque',flashlight:'Lampe',wipe:'Visière',mobiglas:'mobiGlas',inventory:'Inventaire',interact:'Interaction',heal:'Soin',
    starmap:'Carte stellaire',doors:'Ouvrir / fermer portes',doorsOpen:'Ouvrir portes',doorsClose:'Fermer portes',doorunlock:'Déverrouiller portes',doorlock:'Verrouiller portes',camera:'Caméra externe',headtrack:'Head tracking',mfd:'MFD suivant',eject:'Éjection',destruct:'Autodestruction',
    sf:'Bouclier avant',sr:'Bouclier arrière',sl:'Bouclier gauche',sright:'Bouclier droit',stop:'Bouclier haut',sbottom:'Bouclier bas',sreset:'Équilibrer boucliers',salute:'Salut',wave:'Faire signe',respect:'Respect'
  };
  const iconPaths={
    ship:'M12 1 15 8 15 13 21 19 16 18 14 22 12 19 10 22 8 18 3 19 9 13 9 8Z M12 5V16',
    weapons:'M6 4V15M12 3V15M18 4V15M5 19H7M11 19H13M17 19H19',
    engines:'M5 9 12 2 19 9 15 9 12 20 9 9Z M8 6H16',
    shields:'M12 2 21 6 19 15 12 22 5 15 3 6Z M12 6V17',
    coolers:'M12 3C20 3 20 12 12 12C12 4 3 4 3 12C3 20 12 20 12 12C4 12 4 21 12 21C20 21 20 12 12 12Z',
    power:'M12 2V11 M7 5A9 9 0 1 0 17 5',
    up:'M5 14 12 7 19 14 M5 19 12 12 19 19',down:'M5 5 12 12 19 5 M5 10 12 17 19 10',
    gear:'M9 2H15M12 2V14M9 8H15M12 13 5 21H19Z M8 18H16',
    vtol:'M3 15 7 8 12 13 17 8 21 15 M4 5H8M16 5H20',
    landing:'M5 4H9V20H5Z M15 4H19V20H15Z M11 8H13V16H11',
    master:'M3 12 8 7 13 12 8 17Z M15 12H23 M19 8V16',
    brake:'M4 9 12 3 20 9 12 15Z M5 15 12 21 19 15',
    lock:'M4 9V4H9M15 4H20V9M20 15V20H15M9 20H4V15 M8 8 16 16M16 8 8 16',
    hostile:'M12 2 22 12 12 22 2 12Z M12 8 16 12 12 16 8 12Z',
    subtarget:'M8 4 1 12 8 20M16 4 23 12 16 20M12 6 18 12 12 18 6 12Z',
    pin:'M3 8V3H8M16 3H21V8M21 16V21H16M8 21H3V16 M16 12A4 4 0 1 0 8 12A4 4 0 1 0 16 12',
    camera:'M3 7H8L10 4H15L17 7H21V20H3Z M16 13A4 4 0 1 0 8 13A4 4 0 1 0 16 13',
    ping:'M2 9Q12 -2 22 9M5 12Q12 4 19 12M8 15Q12 10 16 15M11 19H13',
    lights:'M3 10 8 7V17L3 14Z M10 9H14V15H10 M17 6 20 3M17 12H23M17 18 20 21',
    mobiglas:'M3 7H21V17H3Z M6 4H18M6 20H18M7 10H17V14H7Z',
    starmap:'M12 2 14 9 21 12 14 14 12 22 10 14 2 12 10 9Z M20 2V6M18 4H22',
    headtrack:'M3 12A9 9 0 1 0 21 12A9 9 0 1 0 3 12 M4 10H20M12 10V21 M8 14H16',
    helmet:'M4 13V10A8 8 0 0 1 20 10V16L16 21H8L4 16Z M5 10H19L17 15H7Z',
    inventory:'M12 3 17 6 12 9 7 6Z M6 11 11 14 6 17 1 14Z M18 11 23 14 18 17 13 14Z M6 17V21M18 17V21',
    doors:'M4 3H20V21H4Z M12 3V21 M9 11V14M15 11V14',
    missile:'M5 19 17 5 21 3 19 8 8 20Z M10 12 4 11 2 15 7 16 M12 14 13 21 17 19 16 10',
    eject:'M5 18 10 6 16 6 13 15 19 18 M11 2H17 M4 22H20 M4 9 2 13M20 3 22 7',
    destruct:'M4 10A8 8 0 0 1 20 10V15L16 17V21H8V17L4 15Z M7 10H10V13H7Z M14 10H17V13H14Z M12 16V19',
    tool:'M4 20 17 7M14 3 21 10M3 16 8 21',
    emote:'M12 2A3 3 0 1 0 12 8A3 3 0 1 0 12 2 M12 8V17M5 11 12 13 20 5M12 17 7 23M12 17 17 23',
    reload:'M4 9A8 8 0 1 1 5 18 M4 3V9H10',
    heal:'M9 3H15V9H21V15H15V21H9V15H3V9H9Z',
    mfd:'M2 6H10V18H2Z M14 6H22V18H14Z M5 9H7M17 9H19',
    wave:'M3 12Q6 5 9 12T15 12T21 12',
    generic:'M12 3 21 12 12 21 3 12Z M9 12H15'
  };
  const iconAlias={pw:'weapons',pt:'engines',ps:'shields',balance:'power',startup:'ship',autoland:'gear',flashlight:'lights',wipe:'helmet',helmetstow:'helmet',freecam:'camera',decoy:'shields',noise:'wave',scan:'ping',friendly:'hostile',attacker:'hostile',cycleall:'hostile',subreset:'subtarget',pin2:'pin',arm:'missile',primary:'weapons',secondary:'missile',sidearm:'missile',melee:'tool',reload2:'reload',lower:'down',zeroUp:'pin',zeroDown:'pin',cruise:'ping',decoupled:'hostile',gsafe:'ship',esp:'lock',limiter:'engines',proximity:'ping',lightamp:'heal',mfdprev:'mfd',sf:'shields',sr:'shields',sl:'shields',sright:'shields',stop:'shields',sbottom:'shields',sreset:'ship'};
  function icon(id){const key=id.endsWith('Up')?'up':id.endsWith('Down')?'down':iconAlias[id]||id;return `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.65" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="${iconPaths[key]||iconPaths.generic}"/></svg>`;}
  const quickFlight=['gear','vtol','landing','master','lights','scan','ping','decoy','cruise','decoupled','mobiglas','starmap'];
  const quickFoot=['mobiglas','inventory','flashlight','helmet','interact','heal','starmap'];
  const systemGroups={
    'PILOTAGE':['brake','limiter','gsafe','esp','autoland','cruise','decoupled','master','gear','vtol','landing'],
    'ÉNERGIE':['power','engines','weapons','shields','coolers','pw','pt','ps','balance'],
    'DISTRIBUTION':['weaponsUp','weaponsDown','weaponsMax','weaponsMin','enginesUp','enginesDown','enginesMax','enginesMin','shieldsUp','shieldsDown','shieldsMax','shieldsMin','balance'],
    'PERSONNEL':['mobiglas','inventory','flashlight','helmet','helmetstow','wipe','interact','heal','primary','secondary','sidearm','reload','firemode','freecam','salute','wave','respect'],
    'SYSTÈMES':['lights','doors','doorsOpen','doorsClose','doorunlock','doorlock','camera','headtrack','mfd','mobiglas','starmap','scan','ping','eject','destruct']
  };
  const combatGroups={
    'CIBLAGE':['lock','hostile','attacker','friendly','subtarget','pin'],
    'ARMES':['weapons','group','missile','arm'],
    'DÉFENSE':['decoy','noise'],
    'CAPTEURS':['scan','ping']
  };
  let powerTab='power',targetTab='target';
  let page='Home',state=null,ws=null,port=(['127.0.0.1','localhost'].includes(location.hostname)&&location.port?Number(location.port):32147),retry=null,backoff=800,connected=false,lastContext='',hold=null,ping=null,lastMessage=0,renderKey='';
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
        if(state.context!==lastContext){targetTab=state.context==='MINING'?'mine':state.context==='SALVAGE'?'salvage':'target';}lastContext=state.context;if(!availablePages().includes(page))page='Home';update();
      } else if(msg.type==='ack'){const p=pending.get(msg.requestId);if(p){clearTimeout(p.timeout);pending.delete(msg.requestId);msg.ok?p.resolve(msg):p.reject(Error(msg.error||'Commande refusée'));}}
    };
    socket.onerror=()=>socket.close();socket.onclose=()=>{if(socket!==ws)return;disconnect();toast('Companion indisponible · nouvelle tentative automatique.');retry=setTimeout(connect,backoff);backoff=Math.min(backoff*1.6,10000);};
  }
  function unusedAvailablePages(){
    if(!state||!state.running||state.context==='UNKNOWN')return ['Home','Settings'];
    if(state.context==='ON_FOOT')return ['Home','FPS','Settings'];
    if(state.context==='GROUND_VEHICLE')return ['Home','Systems','Settings'];
    return ['Home','Combat','Systems','Settings'];
  }
  function availablePages(){return ['Home','Combat','Systems','FPS','Settings','Bindings','Indicators'];}
  function nav(){const el=$('nav');el.replaceChildren();availablePages().forEach(p=>{const [label,g]=pages[p];const b=document.createElement('button');b.textContent=label;b.dataset.glyph=g;b.className=p===page?'active':'';b.onclick=()=>{page=p;renderKey='';render();};el.append(b);});}
  function ctxLabel(v){return({UNKNOWN:'INCONNU',ON_FOOT:'À PIED',FLIGHT:'VOL',GROUND_VEHICLE:'VÉHICULE',MINING:'MINAGE',SALVAGE:'SALVAGE'})[v]||v||'INCONNU';}
  function applyAppearance(){
    const root=document.documentElement,appearance=state?.appearance||{};
    root.dataset.theme=appearance.theme||state?.theme||'nebula';
    root.dataset.density=appearance.density||'comfortable';
    root.dataset.animations=appearance.animations===false?'off':'on';
    const color=(v,f)=>/^#[0-9a-f]{6}$/i.test(v||'')?v:f;
    root.style.setProperty('--accent',color(appearance.accent||state?.accent,'#00c8fa'));
    root.style.setProperty('--accent2',color(appearance.accent2,'#2b8cff'));
    root.style.setProperty('--bg',color(appearance.background,'#061019'));
    root.style.setProperty('--panel',appearance.manufacturerColors&&state?.ship?'color-mix(in srgb, '+color(appearance.accent,'#00c8fa')+' 22%, #05121a)':color(appearance.panel,'#063044'));
    root.style.setProperty('--panel-opacity',`${Math.max(60,Math.min(100,appearance.panelOpacity??92))}%`);
    root.style.setProperty('--radius',`${Math.max(0,Math.min(20,appearance.radius??2))}px`);
    root.style.setProperty('--font-scale',String(Math.max(.85,Math.min(1.2,(appearance.fontScale??100)/100))));
    root.style.setProperty('--glow-alpha',String(Math.max(0,Math.min(1,(appearance.glow??32)/100))));

  }
  function update(){
    applyAppearance();$('connection').textContent='CONNECTÉ';$('game').textContent=state.running?'EN COURS':'ARRÊTÉ';$('branchBuild').textContent=state.running?`${state.branch} ${shortBuild(state.build)}`:'—';$('logState').textContent=state.logFound?'TROUVÉ':'ABSENT';$('footerStatus').textContent=state.running?`${state.branch} · ${state.build} · ${state.shard||'shard en attente'}`:'STAR CITIZEN ARRÊTÉ';render();
  }
  const shortBuild=v=>!v||v==='Unavailable'?'—':String(v).split('-')[0].slice(0,14);
  function render(){if(['Settings','Bindings','Indicators'].includes(page)&&renderKey&&$('content').childElementCount&&document.activeElement?.matches('input,select')){updateHeader();return;}const key=JSON.stringify([page,connected,state?.running,state?.context,state?.actions,state?.feed,state?.location,state?.shard,state?.build,state?.ship,state?.appearance,state?.update,state?.economy,state?.powerAdjustments,powerTab,targetTab]);if(key===renderKey)return;if(hold){updateHeader();return;}renderKey=key;cancelHold();nav();updateHeader();const c=$('content');c.replaceChildren();if(page==='Home')home(c);else if(page==='Combat')groupPage(c,'COMBAT & CIBLAGE','Les commandes utiles au combat, regroupées sans doublons.',combatGroups);else if(page==='Systems')groupPage(c,'SYSTÈMES DU VAISSEAU','Pilotage avancé, énergie, boucliers et systèmes secondaires.',systemGroups);else if(page==='FPS')footPage(c);else if(page==='Bindings')bindingsPage(c);else if(page==='Indicators')indicatorsPage(c);else settings(c);}
  function pageHead(title,sub,context=true){const h=document.createElement('div');h.className='page-head';const left=document.createElement('div');left.innerHTML=`<div class="kicker">ASTERION / ${ctxLabel(state?.context)}</div><h1>${title}</h1><p>${sub}</p>`;h.append(left);if(false){const s=document.createElement('div');s.className='context-switch';for(const [id,label] of [['AUTO','AUTO'],['ON_FOOT','À PIED'],['FLIGHT','VOL']]){const b=document.createElement('button');b.textContent=label;b.className=contextSelected(id)?'selected':'';b.onclick=()=>send('context',id).catch(e=>toast(e.message));s.append(b);}h.append(s);}const back=document.createElement('button');back.className='back-button';back.textContent='RETOUR COCKPIT';back.onclick=()=>{page='Home';renderKey='';render();};h.append(back);return h;}
  function contextSelected(id){if(!state)return false;if(id==='AUTO')return state.contextSource!=='MANUAL'&&state.contextSource!=='SIMULATION';return state.context===id;}
  function selectContext(id){send('context',id).then(()=>{page=id==='ON_FOOT'?'FPS':'Home';renderKey='';render();}).catch(e=>toast(e.message));}
  function updateHeader(){
    document.body.classList.toggle('linked',connected);document.body.dataset.mode=state?.context==='ON_FOOT'?'foot':'ship';
    const foot=state?.context==='ON_FOOT';$('shipIcon').innerHTML=icon(foot?'helmet':'ship');
    $('shipName').textContent=foot?'À pied':state?.ship||'Vaisseau non identifié';
    $('shipDetail').textContent=state?.simulation?'SIMULATION · AUCUNE SAISIE':connected?`${ctxLabel(state?.context)} · ${contextSelected('AUTO')?'AUTO':'MANUEL'}`:'ASTERION / EN ATTENTE';
    $('missionName').textContent=state?.economy?.mission||'Aucun contrat observé';
    $('locationName').textContent=state?.location||'Non observée';$('shardName').textContent=state?.shard||'—';
    const sw=$('contextSwitch');sw.replaceChildren();for(const [id,label] of [['ON_FOOT','À PIED'],['FLIGHT','VAISSEAU'],['AUTO','AUTO']]){const b=document.createElement('button');b.textContent=label;b.className=(contextSelected(id)?'selected ':'')+(id==='AUTO'?'auto-button':'');b.disabled=!connected;b.setAttribute('aria-pressed',contextSelected(id));b.onclick=()=>selectContext(id);sw.append(b);}
    const ticker=$('eventTicker');ticker.replaceChildren();const events=[...(state?.feed||[])].reverse().slice(0,3);if(!events.length)ticker.innerHTML='<span class="empty">◆ ASTERION EDGE · En attente des événements de session</span>';for(const e of events){const row=document.createElement('span');row.className='ticker-item';row.innerHTML=`<b>◆ ${escapeHtml(e.source||'SESSION')}</b>${escapeHtml(e.text)}<time>${escapeHtml(e.time)}</time>`;ticker.append(row);}
  }
  function action(id,label,cls='mfd-action'){
    const found=(state?.actions||[]).find(a=>a.id===id);const a=found||{id,label:label||actionLabels[id]||id,bound:false,source:'NON LIÉ'};
    const b=actionButton(a,cls);if(label)b.querySelector('b').textContent=label;return b;
  }
  function panel(title,sub,extra=''){
    const p=document.createElement('article');p.className='mfd-panel '+extra;
    p.innerHTML=`<div class="mfd-head"><b>${title}</b><i class="head-hatch"></i><small>${sub}</small></div><div class="panel-body"></div>`;return p;
  }
  function grid(ids,extra=''){const g=document.createElement('div');g.className='mfd-grid '+extra;for(const item of ids)g.append(Array.isArray(item)?action(...item):action(item));return g;}
  function tabButtons(options,current,onSelect){const tabs=document.createElement('div');tabs.className='tabs';for(const [id,label] of options){const b=document.createElement('button');b.className='tab '+(id===current?'selected':'');b.textContent=label;b.setAttribute('aria-pressed',id===current);b.onclick=()=>{onSelect(id);renderKey='';render();};tabs.append(b);}return tabs;}
  function home(c){
    if(state?.context==='ON_FOOT'){footDashboard(c);return;}
    const wrap=document.createElement('section');wrap.className='mfd-layout';wrap.setAttribute('aria-label','Cockpit principal');
    wrap.append(powerPanel(),flightPanel(),systemsPanel(),targetPanel(),dangerRail());c.append(wrap);
  }
  function powerPanel(){
    const p=panel('POWER','','power-panel');const body=p.querySelector('.panel-body');
    p.querySelector('.mfd-head small').replaceWith(tabButtons([['power','POINTS'],['systems','SYSTÈMES']],powerTab,v=>powerTab=v));
    if(powerTab==='systems'){
      body.append(grid([['engines','MOTEURS'],['weapons','ARMES'],['shields','BOUCLIERS'],['coolers','COOLING'],['lights','LUMIÈRES'],['doors','PORTES ⇄'],['doorsOpen','OUVRIR'],['doorsClose','FERMER'],['doorunlock','DÉVERROUILLER']],'power-system-grid'));
      const sync=document.createElement('button');sync.className='power-help';sync.textContent='SYNCHRONISER LES INDICATEURS';sync.onclick=()=>{page='Indicators';renderKey='';render();};body.append(sync);return p;
    }
    const hint=document.createElement('div');hint.className='power-caption';hint.textContent='DISTRIBUTION DES POINTS';body.append(hint);
    for(const [id,label] of [['weapons','ARMES'],['engines','MOTEURS'],['shields','BOUCLIERS']]){
      const row=document.createElement('section');row.className='pip-channel';
      const delta=state?.powerAdjustments?.[id]||0;
      row.innerHTML=`<div class="pip-title"><span>${icon(id)} ${label}</span><small>${delta>0?'+':''}${delta} ordres nets</small></div><div class="pip-markers" aria-hidden="true">${Array.from({length:12},(_,i)=>`<i class="${i<Math.min(12,Math.abs(delta))?'changed':''}"></i>`).join('')}</div>`;
      row.append(grid([[id+'Down','− 1 POINT'],[id+'Up','+ 1 POINT'],[id+'Min','MIN'],[id+'Max','MAX']],'pip-actions'));body.append(row);
    }
    const explanation=document.createElement('div');explanation.className='power-caption power-note';explanation.textContent='Traits = ajustements envoyés, pas les points réels du jeu.';body.append(explanation);
    const cooler=document.createElement('div');cooler.className='cooler-adjust';cooler.innerHTML='<span>DÉBIT COOLING</span>';cooler.append(action('coolersDown','−'),action('coolersUp','+'));body.append(cooler);
    const bottom=document.createElement('div');bottom.className='power-bottom';bottom.append(action('startup','DÉMARRAGE'),action('balance','RESET POINTS'));body.append(bottom);return p;
  }
  function section(label,ids,cls=''){const s=document.createElement('section');s.className='flight-section';s.innerHTML=`<div class="section-label">${label}</div>`;s.append(grid(ids,cls));return s;}
  function flightPanel(){const p=panel('FLIGHT','COCKPIT','flight-panel');p.querySelector('.panel-body').append(section('MASTER MODES',[['master','SCM ⇄ NAV'],['brake','FREIN SPATIAL']],'master-grid'),section('ATTERRISSAGE & DOCKING',[['gear','TRAIN'],['autoland','AUTO LAND'],['landing','ATC'],['vtol','VTOL'],['doors','PORTES']],'landing-grid'),section('ASSISTANCES',[['decoupled','DCPL'],['cruise','CRUISE'],['gsafe','G-SAFE'],['esp','ESP'],['proximity','PROX'],['limiter','LIMIT']],'assist-grid'));return p;}
  function systemsPanel(){const p=panel('SHIP SYSTEMS','POWER & DISPLAYS','systems-panel');const body=p.querySelector('.panel-body');for(const [label,ids,cls] of [['ALIMENTATION',[['weapons','ARMES'],['engines','MOTEURS'],['shields','BOUCLIERS']],''],['AFFICHAGES',[['mfdprev','MFD ◂'],['mfd','MFD ▸'],['mobiglas','MOBIGLAS'],['starmap','STARMAP']],'display-grid'],['UTILITAIRES',[['camera','CAMÉRA'],['ping','PING'],['lights','LUMIÈRES'],['scan','SCAN'],['lightamp','VISION NUIT'],['headtrack','HEAD TRACK']],'']]){const s=section(label,ids,cls);s.className='systems-section';body.append(s);}return p;}
  function targetPanel(){const p=panel('MFD',targetTab.toUpperCase(),'target-panel');const body=p.querySelector('.panel-body');const ids=targetTab==='mine'?[['miningmode','MODE MINAGE'],['mininglaser','LASER'],['miningpowerUp','PUISSANCE +'],['miningpowerDown','PUISSANCE −'],['miningmodule1','MODULE 1'],['miningmodule2','MODULE 2'],['miningmodule3','MODULE 3'],['scan','SCAN'],['ping','PING']]:targetTab==='salvage'?[['salvagemode','RÉCUPÉRATION'],['salvagelaser','FAISCEAU'],['salvagecycle','MODE SUIVANT'],['salvagefocus','FOCUS'],['salvagesizeUp','ZONE +'],['salvagesizeDown','ZONE −'],['scan','SCAN'],['ping','PING']]:[['lock','LOCK'],['hostile','HOSTILE'],['friendly','ALLIÉE'],['attacker','ATTAQUANT'],['cycleall','TOUTES'],['subtarget','COMPOSANT'],['subreset','RESET CIBLE'],['pin','PIN 1'],['pin2','PIN 2'],['decoy','LEURRE'],['noise','NOISE'],['arm','ARMER MISSILE']];body.append(grid(ids));const tabs=tabButtons([['target','TARGET'],['mine','MINE'],['salvage','SALVAGE']],targetTab,v=>targetTab=v);tabs.className='target-tabs';body.append(tabs);return p;}
  function dangerRail(){const rail=document.createElement('aside');rail.className='danger-rail';for(const [id,label] of [['eject','ÉJECTION'],['destruct','AUTO DESTRUCT']])rail.append(action(id,label,'danger-tall'));return rail;}
  function footDashboard(c){
    const wrap=document.createElement('section');wrap.className='foot-mfd';wrap.setAttribute('aria-label','Mode à pied');
    const social=panel('ON FOOT','SOCIAL','foot-social');const sb=social.querySelector('.panel-body');const ship=document.createElement('button');ship.className='mfd-action startup';ship.innerHTML=`<span class="action-glyph">${icon('ship')}</span><b>COMMANDES<br>VAISSEAU<small>PASSER EN MODE VOL</small></b>`;ship.disabled=!connected;ship.onclick=()=>selectContext('FLIGHT');sb.append(ship);const power=document.createElement('div');power.className='foot-power';power.append(action('startup','DÉMARRAGE VAISSEAU'));sb.append(power);const label=document.createElement('div');label.className='section-label';label.textContent='EMOTES';sb.append(label,grid([['salute','SALUT'],['wave','SIGNE'],['greet','BONJOUR'],['agree','ACCORD'],['clap','APPLAUDIR'],['cheer','ENCOURAGER'],['laugh','RIRE'],['point','POINTER'],['come','VENIR']],'emote-grid'));
    const suit=panel('SUIT & VIEW','PERSONNEL','foot-panel');suit.querySelector('.panel-body').append(grid([['mobiglas','MOBIGLAS'],['flashlight','LAMPE'],['inventory','INVENTAIRE'],['helmet','CASQUE'],['helmetstow','RANGER CASQUE'],['wipe','VISIÈRE'],['camera','CAMÉRA'],['freecam','CAMÉRA LIBRE'],['starmap','STARMAP']]));
    const weapons=panel('WEAPON','ÉQUIPEMENT','foot-panel');weapons.querySelector('.panel-body').append(grid([['primary','PRINCIPALE'],['secondary','SECONDAIRE'],['sidearm','ARME DE POING'],['melee','MÊLÉE'],['reload','RECHARGER'],['reload2','RECHARGE ALT.'],['lower','BAISSER'],['zeroUp','ZÉRO +'],['zeroDown','ZÉRO −']]));wrap.append(social,suit,weapons,eventPanel());c.append(wrap);
  }
  function eventPanel(){const p=panel('EVENT FEED','GAME.LOG','event-mfd');const list=document.createElement('div');list.className='event-list';const events=[...(state?.feed||[])].reverse();if(!events.length)list.innerHTML='<div class="empty">Aucun événement observé.</div>';for(const e of events){const row=document.createElement('div');row.className='event-line';row.innerHTML=`<time>${escapeHtml(e.time)}</time><span><b>${escapeHtml(e.source||'SESSION')}</b>${escapeHtml(e.text)}</span>`;list.append(row);}p.querySelector('.panel-body').append(list);return p;}
  function quickActions(c){}
  function groupPage(c,title,sub,groups){c.append(pageHead(title,sub));const wrap=document.createElement('section');wrap.className='page-card group-grid';for(const [name,ids] of Object.entries(groups)){const g=document.createElement('div');g.className='group';const gt=document.createElement('div');gt.className='group-title';gt.textContent=name;const grid=document.createElement('div');grid.className='command-grid';const list=ids.map(id=>(state?.actions||[]).find(a=>a.id===id)).filter(Boolean);for(const a of list)grid.append(actionButton(a,'command-card'));if(!list.length){const e=document.createElement('div');e.className='empty';e.textContent='Aucune commande disponible.';grid.append(e);}g.append(gt,grid);wrap.append(g);}c.append(wrap);}
  function footPage(c){footDashboard(c);}
  function actionButton(a,cls){const b=document.createElement('button');b.dataset.action=a.id;b.title=(actionLabels[a.id]||a.label)+' · '+(a.bound?bindingLabel(a.input):'À configurer dans Réglages → Commandes');b.className=cls+(a.dangerous?' danger':'');b.disabled=!connected||(!state?.simulation&&(!a.bound||!state?.running));const binding=state?.simulation?'SIM':a.bound?bindingLabel(a.input):a.source;if(cls==='quick-action'||cls.includes('mfd-action')||cls.includes('shield-action')||cls.includes('danger-tall')){b.innerHTML=`<span class="action-glyph">${icon(a.id)}</span><b>${escapeHtml(actionLabels[a.id]||a.label)}</b><small>${''}${escapeHtml(a.dangerous?'MAINTENIR':state?.simulation?'SIMULATION':!a.bound?'À CONFIGURER':binding)}</small>`;}else{b.innerHTML=`<b>${escapeHtml(actionLabels[a.id]||a.label)}</b><small>${''}${escapeHtml(a.dangerous?'MAINTENIR':state?.simulation?'SIMULATION':!a.bound?'À CONFIGURER':binding)}</small>`;}if(a.feedbackSupported){b.classList.add('has-feedback');if(a.estimatedActive===true)b.classList.add('estimated-on');else if(a.estimatedActive===false)b.classList.add('estimated-off');else if(a.feedbackTouched)b.classList.add('feedback-unknown');const badge=document.createElement('span');badge.className='state-badge';badge.textContent=a.estimatedActive===true?'ON*':a.estimatedActive===false?'OFF*':a.feedbackTouched?'ORDRE ?':'?';badge.title='* Estimation dashboard. Synchronisez dans Réglages → Indicateurs.';b.append(badge);}if(!a.dangerous)b.onclick=()=>execute(a,b);else{const p=document.createElement('span');p.className='progress';b.append(p);b.onpointerdown=e=>startHold(e,a,b,p);b.onpointerup=b.onpointercancel=b.onlostpointercapture=cancelHold;b.oncontextmenu=e=>e.preventDefault();}return b;}
  function startHold(e,a,b,p){if(e.button!==0||b.disabled)return;e.preventDefault();cancelHold();b.setPointerCapture(e.pointerId);const h={button:b,progress:p,id:a.id,start:performance.now(),pulse:null,raf:null};hold=h;b.classList.add('holding');send('holdBegin',a.id).then(()=>{if(hold!==h)return;h.start=performance.now();h.pulse=setInterval(()=>raw({type:'holdPulse'}),150);const frame=()=>{if(hold!==h)return;const elapsed=performance.now()-h.start;p.style.width=Math.min(100,elapsed/(state?.holdDuration||1800)*100)+'%';if(elapsed>=(state?.holdDuration||1800)+80){clearInterval(h.pulse);hold=null;b.classList.remove('holding');p.style.width='0';execute(a,b);}else h.raf=requestAnimationFrame(frame);};h.raf=requestAnimationFrame(frame);}).catch(err=>{cancelHold();toast(err.message);});}
  function cancelHold(){if(!hold)return;clearInterval(hold.pulse);cancelAnimationFrame(hold.raf);hold.button.classList.remove('holding');hold.progress.style.width='0';hold=null;raw({type:'holdCancel'});}
  function execute(a,b){send('action',a.id).then(()=>{toast(state?.simulation?`Simulation : ${actionLabels[a.id]||a.label}`:`Commande envoyée : ${actionLabels[a.id]||a.label}`);b.classList.add('sent');setTimeout(()=>b.classList.remove('sent'),260);}).catch(e=>toast(e.message));}
  const bindingLabel=input=>String(input||'').replace(/^kb1_/i,'').replaceAll('_',' ').toUpperCase();
  function settings(c){c.append(pageHead('PERSONNALISATION','Réglez réellement l’apparence du cockpit : couleurs, densité, typographie, angles et effets.',false));const a=state?.appearance||{};const layout=document.createElement('section');layout.className='settings-layout';const left=document.createElement('div');left.className='settings-column';const presets=card('PRÉSETS','Choisissez une base puis ajustez chaque détail.');const pr=document.createElement('div');pr.className='preset-row';for(const [id,label] of [['nebula','RSI · Cyan'],['tactical','Anvil · Olive'],['amber','Drake · Ambre'],['solar','MISC · Or'],['violet','Violet'],['graphite','Graphite'],['minimal','Minimal']]){const b=document.createElement('button');b.className='preset-btn '+((a.theme||'nebula')===id?'selected':'');b.textContent=label;b.onclick=()=>send('theme',id).then(()=>toast('Preset appliqué.')).catch(e=>toast(e.message));pr.append(b);}presets.append(pr);const colors=card('COULEURS','Palette complète, pas seulement une couleur d’accent.');const cg=document.createElement('div');cg.className='color-grid';for(const [key,label,fallback,type] of [['accent','Accent principal','#00c8fa','accent'],['accent2','Accent secondaire','#2b8cff','accent2'],['background','Fond','#061019','background'],['panel','Panneaux','#063044','panel']]){const box=document.createElement('label');box.className='color-field';box.textContent=label;const input=document.createElement('input');input.type='color';input.value=/^#[0-9a-f]{6}$/i.test(a[key]||'')?a[key]:fallback;input.onchange=()=>send(type,input.value).then(()=>toast(`${label} enregistré.`)).catch(e=>toast(e.message));box.append(input);cg.append(box);}colors.append(cg);const controls=card('AFFICHAGE','Ajustez la lisibilité au XENEON EDGE et à votre distance de jeu.');const cl=document.createElement('div');cl.className='control-list';cl.append(slider('Opacité panneaux',a.panelOpacity??92,60,100,'%','panelOpacity'),slider('Taille du texte',a.fontScale??100,85,120,'%','fontScale'),slider('Angles',a.radius??2,0,20,'px','radius'),slider('Intensité glow',a.glow??32,0,100,'%','glow'));const density=document.createElement('div');density.className='control';density.innerHTML='<label>Densité <span></span></label>';const sel=document.createElement('select');for(const [v,l] of [['compact','Compacte'],['comfortable','Confortable'],['large','Grande']]){const o=document.createElement('option');o.value=v;o.textContent=l;o.selected=(a.density||'comfortable')===v;sel.append(o);}sel.onchange=()=>send('density',sel.value).then(()=>toast('Densité enregistrée.')).catch(e=>toast(e.message));density.append(sel);cl.append(density);controls.append(cl);const toggles=document.createElement('div');toggles.className='toggle-row';toggles.append(toggle('Couleurs constructeur',a.manufacturerColors??state?.manufacturerColors,'manufacturerColors'),toggle('Animations',a.animations!==false,'animations'));controls.append(toggles);left.append(presets,colors,controls);const preview=card('APERÇU','Les changements sont appliqués instantanément et sauvegardés par le Companion.','preview-card');const demo=document.createElement('div');demo.className='preview-demo';demo.innerHTML='<span></span><span></span><span></span>';preview.append(demo);const vr=document.createElement('div');vr.className='version-row';vr.innerHTML=`<span>Version interface</span><strong>v${VERSION}</strong>`;preview.append(vr);const update=document.createElement('button');update.className='update-button';update.textContent=state?.update?.available?`MISE À JOUR v${state.update.latest} DISPONIBLE`:'AUCUNE MISE À JOUR PUBLIQUE';update.disabled=!state?.update?.available;update.onclick=()=>send('openUpdate').catch(e=>toast(e.message));preview.append(update);const tools=document.createElement('div');tools.className='settings-tools';for(const [label,go] of [['COMMANDES & RACCOURCIS','Bindings'],['INDICATEURS / SYNCHRO','Indicators'],['TOUS LES SYSTÈMES','Systems'],['COMBAT','Combat'],['RETOUR COCKPIT','Home']]){const b=document.createElement('button');b.textContent=label;b.onclick=()=>{page=go;renderKey='';render();};tools.append(b);}const diagnostic=document.createElement('p');diagnostic.textContent='Contexte : '+ctxLabel(state?.context)+' · '+(state?.contextSource||'INCONNU')+'. AUTO ne bascule que sur un événement local reconnu dans Game.log.';preview.append(diagnostic,tools);layout.append(left,preview);c.append(layout);}
  function card(title,sub,extra=''){const el=document.createElement('article');el.className='setting-card '+extra;el.innerHTML=`<h3>${title}</h3><p>${sub}</p>`;return el;}
  function slider(label,value,min,max,suffix,type){const box=document.createElement('div');box.className='control';const lab=document.createElement('label');const v=document.createElement('span');v.textContent=`${value}${suffix}`;lab.append(document.createTextNode(label),v);const input=document.createElement('input');input.type='range';input.min=min;input.max=max;input.value=value;input.oninput=()=>v.textContent=`${input.value}${suffix}`;input.onchange=()=>send(type,input.value).then(()=>toast(`${label} enregistré.`)).catch(e=>toast(e.message));box.append(lab,input);return box;}
  function toggle(label,on,type){const b=document.createElement('button');b.className='toggle '+(on?'on':'');b.textContent=`${label} : ${on?'ON':'OFF'}`;b.onclick=()=>send(type,on?'0':'1').then(()=>toast(`${label} mis à jour.`)).catch(e=>toast(e.message));return b;}
  function indicatorsPage(c){
    c.append(pageHead('INDICATEURS DES SYSTÈMES','Renseignez l’état que vous observez en jeu. Les boutons suivront ensuite les commandes envoyées par Asterion.',false));
    const info=document.createElement('p');info.className='indicator-note';info.textContent='ON* / OFF* = estimation, pas télémétrie. Une touche clavier ou une action en jeu peut la désynchroniser. Sans état initial, « ORDRE ? » confirme seulement une commande envoyée.';c.append(info);
    const list=document.createElement('section');list.className='indicator-grid';
    for(const a of (state?.actions||[]).filter(a=>a.feedbackSupported)){
      const row=document.createElement('label');row.className='indicator-row';const name=document.createElement('strong');name.textContent=actionLabels[a.id]||a.label;const select=document.createElement('select');select.setAttribute('aria-label','État observé : '+name.textContent);
      for(const [value,label] of [['unknown','INCONNU'],['on',a.id==='doors'?'OUVERTES':'ALLUMÉ / ACTIVÉ'],['off',a.id==='doors'?'FERMÉES':'ÉTEINT / DÉSACTIVÉ']]){const o=document.createElement('option');o.value=value;o.textContent=label;o.selected=(a.estimatedActive===true?'on':a.estimatedActive===false?'off':'unknown')===value;select.append(o);}
      select.onchange=()=>send('indicator',JSON.stringify({id:a.id,value:select.value==='unknown'?null:select.value==='on'})).then(()=>toast('Indicateur synchronisé avec votre observation.')).catch(e=>toast(e.message));row.append(name,select);list.append(row);
    }c.append(list);const reset=document.createElement('button');reset.className='back-button';reset.textContent='REMETTRE LES INDICATEURS À INCONNU';reset.onclick=()=>send('resetIndicators').then(()=>{renderKey='';render();}).catch(e=>toast(e.message));c.append(reset);
  }
  function bindingsPage(c){
    c.append(pageHead('COMMANDES & RACCOURCIS','Associez les commandes à vos raccourcis en jeu. Exemple : kb1_lalt+n. Aucun raccourci n’est inventé.',false));
    const search=document.createElement('input');search.type='text';search.className='search-input';search.placeholder='Rechercher une commande…';search.setAttribute('aria-label','Rechercher une commande');c.append(search);
    const list=document.createElement('div');list.className='binding-list';c.append(list);
    function draw(){list.replaceChildren();for(const a of (state?.actions||[]).filter(a=>(a.label+' '+(actionLabels[a.id]||'')+' '+a.page).toLowerCase().includes(search.value.toLowerCase()))){
      const row=document.createElement('form');row.className='binding-row';row.dataset.binding=a.id;
      const name=document.createElement('strong');name.textContent=actionLabels[a.id]||a.label;const source=document.createElement('span');source.textContent=a.source;
      const lab=document.createElement('label');lab.textContent='Raccourci clavier';const input=document.createElement('input');input.type='text';input.value=a.input||'';input.placeholder='kb1_lalt+n';input.setAttribute('aria-label','Raccourci '+name.textContent);lab.append(input);
      const duration=document.createElement('label');duration.textContent='Durée (ms)';const ms=document.createElement('input');ms.type='number';ms.min=30;ms.max=1500;ms.value=a.pressMs||90;duration.append(ms);
      const save=document.createElement('button');save.type='submit';save.textContent='ENREGISTRER';save.disabled=!connected;const reset=document.createElement('button');reset.type='button';reset.textContent='RÉINITIALISER';reset.disabled=!connected;
      row.onsubmit=e=>{e.preventDefault();send('binding',JSON.stringify({id:a.id,input:input.value.trim(),pressMs:Number(ms.value)})).then(()=>toast('Raccourci enregistré : '+name.textContent)).catch(e=>toast(e.message));};
      reset.onclick=()=>send('binding',JSON.stringify({id:a.id,input:null})).then(()=>{renderKey='';render();toast('Raccourci réinitialisé.');}).catch(e=>toast(e.message));row.append(name,source,lab,duration,save,reset);list.append(row);
    }}search.oninput=draw;draw();
  }
  function notice(text){const n=document.createElement('div');n.className='notice';n.textContent=text;return n;}
  function escapeHtml(v){return String(v??'').replace(/[&<>'"]/g,ch=>({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[ch]));}
  $('settingsButton').onclick=()=>{page=page==='Settings'?'Home':'Settings';renderKey='';render();};
  $('retryButton').onclick=()=>{backoff=800;connect();};
  window.addEventListener('blur',cancelHold);document.addEventListener('visibilitychange',()=>{if(document.hidden)cancelHold();});window.addEventListener('pagehide',()=>{cancelHold();clearTimeout(retry);clearInterval(ping);ws?.close();});window.addEventListener('resize',()=>{renderKey='';render();});
  setInterval(()=>{$('clock').textContent=new Date().toLocaleTimeString('fr-FR',{hour:'2-digit',minute:'2-digit'});},1000);
  render();connect();
})();
