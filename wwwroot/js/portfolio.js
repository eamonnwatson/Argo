// Argo Portfolio page script
// Depends on common.js (window.Argo) being loaded first.

    (function(){
      "use strict";

      var TEAM_MEMBERS=["Garrett Rowley","Jet Pineda","Korrey Mathews","Jonalyn Flores","Aprilyn Samson"];
      var PRIORITY_RANK={"Needs Triage":5,Critical:4,High:3,Medium:2,Low:1};
      var waitingSort="priority-desc";

      var cleanSeed={projects:[],workItems:[],activities:[],raidItems:[]};
      var data=clone(cleanSeed);
      var selectedId="";
      var expandedWorkIds={};

      function clone(value){return JSON.parse(JSON.stringify(value));}
      async function apiGet(path){
        var res=await fetch(Argo.API_BASE+path);
        if(!res.ok)throw new Error("GET "+path+" failed ("+res.status+")");
        return res.status===204?null:res.json();
      }
      async function apiSend(method,path,body){
        var res=await fetch(Argo.API_BASE+path,{method:method,headers:{"Content-Type":"application/json"},body:body===undefined?undefined:JSON.stringify(body)});
        if(!res.ok)throw new Error(method+" "+path+" failed ("+res.status+")");
        return res.status===204?null:res.json();
      }
      function normalize(value){
        var safe=value&&typeof value==="object"?value:{};
        safe.projects=(Array.isArray(safe.projects)?safe.projects:[]).map(function(project){
          var normalized=Object.assign({priority:"Medium",sourceRequestId:"",submittedAt:"",intakeDetails:null},project);
          if(typeof normalized.intakeDetails==="string"){
            try{normalized.intakeDetails=normalized.intakeDetails?JSON.parse(normalized.intakeDetails):null;}catch(error){normalized.intakeDetails=null;}
          }
          return normalized;
        });
        safe.workItems=(Array.isArray(safe.workItems)?safe.workItems:[]).map(function(item){return Object.assign({purpose:"",participants:"",requiredInputs:"",milestone:"",definitionOfDone:""},item);});
        safe.activities=Array.isArray(safe.activities)?safe.activities:[];
        safe.raidItems=Array.isArray(safe.raidItems)?safe.raidItems:[];
        return safe;
      }
      async function loadPortfolio(){
        try{return normalize(await apiGet("/portfolio"));}
        catch(error){console.error(error);Argo.toast("Could not reach the Argo server \u2014 showing an empty board");return clone(cleanSeed);}
      }
      async function refresh(){data=await loadPortfolio();}
      function dateLabel(value){if(!value)return "No date";var date=new Date(value+"T12:00:00");return date.toLocaleDateString("en-US",{month:"short",day:"numeric",year:"numeric"});}
      function nextId(prefix,list){var max=list.reduce(function(value,id){var number=Number(String(id).split("-").pop());return isFinite(number)?Math.max(value,number):value;},0);return prefix+"-"+String(max+1).padStart(3,"0");}
      function selected(){return data.projects.find(function(project){return project.id===selectedId;})||data.projects[0];}
      function healthClass(value){return String(value).replace(/ /g,"-");}
      function statusClass(value){return "status-"+String(value||"Not Started").replace(/ /g,"-");}
      function priorityClass(value){return String(value||"Medium").replace(/ /g,"-");}
      function ownerMatches(projectOwner,teamMember){
        var owner=String(projectOwner||"").trim().toLowerCase();
        var member=String(teamMember||"").trim().toLowerCase();
        return owner===member||owner===member.split(" ")[0];
      }
      function chartBucket(project){
        if(project.status==="Done")return "Done";
        if(project.health==="Blocked")return "Blocked";
        if(project.health==="At Risk")return "At Risk";
        return project.status==="In Progress"?"In Progress":"Waiting";
      }
      function displayText(value,fallback){return Argo.escapeHtml(value||fallback||"Not entered");}
      function render(){renderSummary();renderTeamChart();renderBoard();renderDetail();}
      function renderSummary(){
        var statuses=["Waiting","In Progress","Done"];
        document.getElementById("summary").innerHTML=statuses.map(function(status){
          var count=data.projects.filter(function(project){return project.status===status;}).length;
          return '<div class="stat"><span class="stat-label">'+Argo.escapeHtml(status)+'</span><span class="stat-value">'+count+'</span></div>';
        }).join("");
      }
      function renderTeamChart(){
        var buckets=["Waiting","In Progress","At Risk","Blocked","Done"];
        var unassigned=data.projects.filter(function(project){return project.status!=="Done"&&String(project.owner).toLowerCase()==="unassigned";}).length;
        var rows=TEAM_MEMBERS.map(function(member){
          var projects=data.projects.filter(function(project){return ownerMatches(project.owner,member);});
          var grouped={};
          buckets.forEach(function(bucket){grouped[bucket]=projects.filter(function(project){return chartBucket(project)===bucket;});});
          return {member:member,projects:projects,grouped:grouped};
        });
        var maxTotal=Math.max.apply(null,rows.map(function(row){return row.projects.length;}));
        if(!isFinite(maxTotal)||maxTotal<1)maxTotal=1;
        var legend=(unassigned?'<span class="triage-count"><strong>'+unassigned+'</strong> awaiting assignment</span>':"")+buckets.map(function(bucket){return '<span class="legend-item"><span class="legend-dot chart-'+healthClass(bucket)+'"></span>'+Argo.escapeHtml(bucket)+'</span>';}).join("");
        var bars=rows.map(function(row){
          var segments=buckets.map(function(bucket){
            var matches=row.grouped[bucket];
            if(!matches.length)return "";
            var width=matches.length/maxTotal*100;
            var names=matches.map(function(project){return project.name;}).join("; ");
            return '<span class="owner-segment chart-'+healthClass(bucket)+'" style="width:'+width+'%" title="'+Argo.escapeHtml(bucket+': '+names)+'">'+matches.length+'</span>';
          }).join("");
          return '<div class="owner-bar-row"><div class="owner-name">'+Argo.escapeHtml(row.member)+'</div><div class="owner-track" aria-label="'+Argo.escapeHtml(row.member+': '+row.projects.length+' assigned projects')+'">'+segments+'</div><div class="owner-total">'+row.projects.length+'<span>assigned</span></div></div>';
        }).join("");
        document.getElementById("team-chart").innerHTML='<div class="chart-head"><div><h2>Team workload &amp; project health</h2><p>Each project appears once. At Risk and Blocked take precedence so attention items remain visible.</p></div><div class="chart-legend">'+legend+'</div></div><div class="owner-bars">'+bars+'</div>';
      }
      function renderBoard(){
        var lanes=[{status:"Waiting",hint:"Submitted, triaged, or queued for work"},{status:"In Progress",hint:"Work is underway"},{status:"Done",hint:"Deliverables accepted"}];
        document.getElementById("board").innerHTML=lanes.map(function(lane){
          var projects=data.projects.filter(function(project){return project.status===lane.status;});
          if(lane.status==="Waiting")projects.sort(function(a,b){
            var difference=(PRIORITY_RANK[b.priority]||2)-(PRIORITY_RANK[a.priority]||2);
            if(waitingSort==="priority-asc")difference=-difference;
            return difference||String(a.name).localeCompare(String(b.name));
          });
          var cards=projects.map(function(project){
            var priority=lane.status==="Waiting"?'<span class="badge priority '+priorityClass(project.priority)+'">'+Argo.escapeHtml(project.priority||"Medium")+'</span>':"";
            return '<button type="button" class="project-card '+(project.id===selectedId?'selected':'')+'" data-action="select-project" data-id="'+Argo.escapeHtml(project.id)+'"><div class="card-top"><span class="project-id">'+Argo.escapeHtml(project.id)+'</span><span class="card-tags">'+priority+'<span class="badge health '+healthClass(project.health)+'">'+Argo.escapeHtml(project.health)+'</span></span></div><h3>'+Argo.escapeHtml(project.name)+'</h3><div class="owner-date"><span class="truncate">◉ '+Argo.escapeHtml(project.owner)+'</span><span>◷ '+Argo.escapeHtml(dateLabel(project.targetDate))+'</span></div><div class="card-foot"><span class="truncate">'+Argo.escapeHtml(project.nextMilestone||"No milestone set")+'</span><span>›</span></div></button>';
          }).join("")||'<div class="empty">No projects</div>';
          var tools=lane.status==="Waiting"?'<div class="lane-tools"><select class="sort-select" data-action="sort-waiting" aria-label="Sort Waiting projects"><option value="priority-desc" '+(waitingSort==="priority-desc"?'selected':'')+'>Priority: highest first</option><option value="priority-asc" '+(waitingSort==="priority-asc"?'selected':'')+'>Priority: lowest first</option></select><span class="count">'+projects.length+'</span></div>':'<span class="count">'+projects.length+'</span>';
          return '<div><div class="lane-head"><div><h2>'+Argo.escapeHtml(lane.status)+'</h2><p>'+Argo.escapeHtml(lane.hint)+'</p></div>'+tools+'</div><div class="cards">'+cards+'</div></div>';
        }).join("");
      }
      function renderWorkPackage(item){
        var activities=data.activities.filter(function(activity){return activity.workItemId===item.id;});
        var completed=activities.filter(function(activity){return activity.status==="Done";}).length;
        var percent=activities.length?Math.round(completed/activities.length*100):0;
        var isExpanded=Boolean(expandedWorkIds[item.id]);
        var activityRows=activities.map(function(activity){
          return '<div class="activity-row"><div><div class="activity-title">'+Argo.escapeHtml(activity.title)+'</div>'+(activity.notes?'<div class="activity-meta">'+Argo.escapeHtml(activity.notes)+'</div>':'')+'</div><div><span class="work-summary-label">Owner</span><span class="activity-meta">'+Argo.escapeHtml(activity.owner)+'</span></div><div><span class="badge '+statusClass(activity.status)+'">'+Argo.escapeHtml(activity.status)+'</span></div><div class="activity-meta">'+Argo.escapeHtml(dateLabel(activity.dueDate))+'</div><button class="edit-link" type="button" data-action="edit-activity" data-id="'+Argo.escapeHtml(activity.id)+'">Edit</button></div>';
        }).join("")||'<div class="empty">No activities yet. Add the first trackable step for this wave.</div>';
        var detail=isExpanded?'<div class="work-detail"><div class="work-detail-grid"><div class="work-detail-card full"><div class="fact-label">Purpose / outcome</div><p>'+displayText(item.purpose)+'</p></div><div class="work-detail-card"><div class="fact-label">Participants</div><p>'+displayText(item.participants)+'</p></div><div class="work-detail-card"><div class="fact-label">Required inputs</div><p>'+displayText(item.requiredInputs)+'</p></div><div class="work-detail-card"><div class="fact-label">Milestone / deliverable</div><p>'+displayText(item.milestone)+'</p></div><div class="work-detail-card"><div class="fact-label">Definition of done</div><p>'+displayText(item.definitionOfDone)+'</p></div><div class="work-detail-card full"><div class="fact-label">Dependency / blocker</div><p>'+displayText(item.dependency,"None entered")+'</p></div></div><div class="activity-section"><div class="activity-head"><div><h4>Wave activities</h4><div class="activity-meta">'+completed+' of '+activities.length+' complete ('+percent+'%)</div><div class="progress-track"><div class="progress-bar" style="width:'+percent+'%"></div></div></div><button class="btn small" type="button" data-action="add-activity" data-work-id="'+Argo.escapeHtml(item.id)+'">＋ Add activity</button></div><div class="activity-list">'+activityRows+'</div></div></div>':'';
        return '<article class="work-package '+(isExpanded?'expanded':'')+'"><div class="work-summary"><button class="work-toggle" type="button" data-action="toggle-work" data-id="'+Argo.escapeHtml(item.id)+'" aria-expanded="'+isExpanded+'"><span class="chevron">›</span><span><span class="work-title">'+Argo.escapeHtml(item.title)+'</span><span class="work-id">'+Argo.escapeHtml(item.id)+' · '+completed+'/'+activities.length+' activities complete</span></span></button><div><span class="work-summary-label">Owner</span><span class="work-summary-value">'+Argo.escapeHtml(item.owner)+'</span></div><div><span class="work-summary-label">Status</span><span class="badge '+statusClass(item.status)+'">'+Argo.escapeHtml(item.status)+'</span></div><div><span class="work-summary-label">Due</span><span class="work-summary-value">'+Argo.escapeHtml(dateLabel(item.dueDate))+'</span></div><div class="work-actions"><button class="edit-link" type="button" data-action="edit-work" data-id="'+Argo.escapeHtml(item.id)+'">Edit</button></div></div>'+detail+'</article>';
      }
      function intakeValue(value){return Array.isArray(value)?value.join(", "):String(value||"");}
      function intakeRow(label,value){var shown=intakeValue(value);if(!shown)return "";return '<div class="intake-row"><div class="intake-label">'+Argo.escapeHtml(label)+'</div><div class="intake-value">'+Argo.escapeHtml(shown)+'</div></div>';}
      function intakeSection(title,rows,size){var content=rows.map(function(row){return intakeRow(row[0],row[1]);}).join("");if(!content)return "";return '<section class="intake-section '+(size||"")+'"><h4>'+Argo.escapeHtml(title)+'</h4><div class="intake-rows">'+content+'</div></section>';}
      function renderIntakePanel(project){
        var d=project.intakeDetails;if(!d)return "";
        var submitted=project.submittedAt?new Date(project.submittedAt).toLocaleString("en-US",{month:"short",day:"numeric",year:"numeric",hour:"numeric",minute:"2-digit"}):"";
        var html="";
        html+=intakeSection("Request and ownership",[["Request ID",project.sourceRequestId||d.requestId],["Submitted",submitted],["Request type",d.requestType],["Requester",d.requesterName],["Department / team",d.department],["Contact",d.requesterContact],["Business sponsor",d.businessSponsor],["Business owner",d.businessOwner],["Additional stakeholders",d.additionalStakeholders]],"");
        html+=intakeSection("Business need",[["Request description",d.requestDescription],["Problem / opportunity",d.businessProblem],["Desired outcome",d.desiredOutcome],["Success measures",d.successMeasures],["Affected groups",d.affectedGroups],["Current workaround",d.currentProcess]],"wide");
        html+=intakeSection("Impact and timing",[["Business impact",d.businessImpact],["Reach",d.impactScope],["Users affected",d.usersAffected],["Client impact",d.clientImpact],["Clients / logos",d.clientNames],["Expected benefits",d.expectedBenefits],["If nothing changes",d.noActionImpact],["Desired date",d.desiredDate],["Date type",d.dateType],["Why the date matters",d.dateReason]],"");
        html+=intakeSection("Scope and dependencies",[["In scope",d.inScope],["Out of scope",d.outOfScope],["Dependencies",d.dependencies],["Strategic alignment",d.strategicAlignment]],"");
        html+=intakeSection("Systems and data",[["Systems / tools",d.systemsInvolved],["Data sources",d.dataSources],["Sensitive data",d.sensitiveData],["Data concern",d.sensitiveDetails],["Access needed",d.accessNeeded],["Technical owners",d.technicalOwners],["Vendors / partners",d.vendors],["Supporting materials",d.supportingMaterials]],"wide");
        if(d.requestType==="Reporting or dashboard request")html+=intakeSection("Reporting details",[["Reports",d.reportNames],["Frequency",d.reportFrequency],["Delivery time",d.deliveryTime],["Recipients",d.reportRecipients],["Output format",d.outputFormat],["Samples available",d.samplesAvailable],["Sample references",d.sampleReferences],["Manual steps",d.manualSteps]],"full");
        if(d.requestType==="Data integration/source request")html+=intakeSection("Data integration details",[["Source system",d.sourceSystem],["Target system",d.targetSystem],["Data owner",d.dataOwner],["Refresh frequency",d.refreshFrequency],["Volume / history",d.dataVolume]],"full");
        return '<details class="intake-panel" open><summary><span>Submitted request · '+Argo.escapeHtml(project.sourceRequestId||d.requestId||"Request")+'</span><span>Original intake details</span></summary><div class="intake-content">'+html+'</div></details>';
      }
      function renderDetail(){
        var project=selected();
        var detail=document.getElementById("detail");
        if(!project){detail.innerHTML='<div class="empty">Add a project to begin.</div>';return;}
        var work=data.workItems.filter(function(item){return item.projectId===project.id;});
        var raid=data.raidItems.filter(function(item){return item.projectId===project.id;});
        var completed=work.filter(function(item){return item.status==="Done";}).length;
        var progress=work.length?Math.round(completed/work.length*100):0;
        var packages=work.map(renderWorkPackage).join("")||'<div class="empty">No work packages yet.</div>';
        var raidCards=raid.map(function(item){return '<button type="button" class="raid-card" data-action="edit-raid" data-id="'+Argo.escapeHtml(item.id)+'"><span class="badge">'+Argo.escapeHtml(item.type)+'</span><span class="raid-copy"><strong>'+Argo.escapeHtml(item.description)+'</strong><span class="raid-meta"><span>'+Argo.escapeHtml(item.owner)+'</span><span>'+Argo.escapeHtml(dateLabel(item.dueDate))+'</span></span></span><span>✎</span></button>';}).join("")||'<div class="empty">No RAID records yet.</div>';
        var priorityLabel=project.priority==="Needs Triage"?"Needs Triage":(project.priority||"Medium")+" priority";
        var priorityTag=project.status==="Waiting"?'<span class="badge priority '+priorityClass(project.priority)+'">'+Argo.escapeHtml(priorityLabel)+'</span>':"";
        detail.innerHTML='<div class="detail-head"><div class="detail-title-row"><div><div class="detail-tags"><span class="project-id" style="color:var(--cyan)">'+Argo.escapeHtml(project.id)+'</span><span class="badge">'+Argo.escapeHtml(project.status)+'</span>'+priorityTag+'<span class="badge health '+healthClass(project.health)+'">'+Argo.escapeHtml(project.health)+'</span></div><h2>'+Argo.escapeHtml(project.name)+'</h2><p class="objective">'+Argo.escapeHtml(project.objective||"No objective entered.")+'</p></div><button class="btn" type="button" data-action="edit-project" data-id="'+Argo.escapeHtml(project.id)+'">✎ Edit project</button></div><div class="facts"><div><div class="fact-label">Project owner</div><div class="fact-value">'+Argo.escapeHtml(project.owner)+'</div></div><div><div class="fact-label">Next milestone</div><div class="fact-value">'+Argo.escapeHtml(project.nextMilestone||"Not set")+'</div></div><div><div class="fact-label">Target date</div><div class="fact-value">'+Argo.escapeHtml(dateLabel(project.targetDate))+'</div></div><div><div class="fact-label">Waves completed</div><div class="fact-value">'+progress+'% ('+completed+'/'+work.length+')</div></div></div></div><div class="detail-body"><div><div class="section-title"><div><h3>Work packages and waves</h3><p>Expand each item for participants, inputs, milestones, completion criteria, and child activities.</p></div><button class="btn small" type="button" data-action="add-work">＋ Add wave</button></div><div class="hint-box">Select a wave to open its working details. Activities are linked through both the project ID and work-item ID.</div><div class="work-list">'+packages+'</div></div><div><div class="section-title"><div><h3>RAID and decisions</h3><p>What needs attention or leadership input</p></div><button class="btn small" type="button" data-action="add-raid">＋ Add record</button></div><div class="raid-list">'+raidCards+'</div></div></div>';
        var editButton=detail.querySelector('[data-action="edit-project"]');
        if(editButton){var actionGroup=document.createElement("div");actionGroup.className="detail-actions";editButton.replaceWith(actionGroup);actionGroup.appendChild(editButton);actionGroup.insertAdjacentHTML("beforeend",'<button class="btn danger" type="button" data-action="delete-project" data-id="'+Argo.escapeHtml(project.id)+'">Delete project</button>');}
        if(project.intakeDetails)detail.querySelector(".detail-head").insertAdjacentHTML("afterend",renderIntakePanel(project));
      }

      function fillForm(form,record){form.reset();Array.from(form.elements).forEach(function(el){if(el.name&&record[el.name]!==undefined)el.value=record[el.name];});}
      function formObject(form){var obj={};new FormData(form).forEach(function(value,key){obj[key]=String(value);});return obj;}
      function syncPriorityField(){document.getElementById("project-priority-field").hidden=document.getElementById("project-status").value!=="Waiting";}
      function openProject(id){var record=id?data.projects.find(function(item){return item.id===id;}):{id:nextId("PRJ",data.projects.map(function(item){return item.id;})),name:"",owner:"",status:"Waiting",health:"On Track",priority:"Medium",objective:"",nextMilestone:"",targetDate:""};fillForm(document.getElementById("project-form"),record);syncPriorityField();document.getElementById("project-dialog-title").textContent=id?"Project "+id:"New project";document.getElementById("project-dialog").showModal();}
      function openWork(id){var project=selected();var record=id?data.workItems.find(function(item){return item.id===id;}):{id:nextId("WI",data.workItems.map(function(item){return item.id;})),projectId:project.id,title:"",owner:"",status:"Not Started",dueDate:"",dependency:"",purpose:"",participants:"",requiredInputs:"",milestone:"",definitionOfDone:""};fillForm(document.getElementById("work-form"),record);document.getElementById("work-dialog").showModal();}
      function openActivity(workItemId,id){var project=selected();var record=id?data.activities.find(function(item){return item.id===id;}):{id:nextId("ACT",data.activities.map(function(item){return item.id;})),projectId:project.id,workItemId:workItemId,title:"",owner:"",status:"Not Started",dueDate:"",notes:""};fillForm(document.getElementById("activity-form"),record);document.getElementById("activity-dialog").showModal();}
      function openRaid(id){var project=selected();var record=id?data.raidItems.find(function(item){return item.id===id;}):{id:nextId("RAID",data.raidItems.map(function(item){return item.id;})),projectId:project.id,type:"Risk",description:"",owner:"",dueDate:""};fillForm(document.getElementById("raid-form"),record);document.getElementById("raid-dialog").showModal();}
      async function deleteProject(id){
        var project=data.projects.find(function(item){return item.id===id;});
        if(!project)return;
        var linkedWorkIds=data.workItems.filter(function(item){return item.projectId===id;}).map(function(item){return item.id;});
        var workCount=linkedWorkIds.length;
        var activityCount=data.activities.filter(function(item){return item.projectId===id||linkedWorkIds.indexOf(item.workItemId)>=0;}).length;
        var raidCount=data.raidItems.filter(function(item){return item.projectId===id;}).length;
        var linked=workCount+" wave"+(workCount===1?"":"s")+", "+activityCount+" activit"+(activityCount===1?"y":"ies")+", and "+raidCount+" RAID record"+(raidCount===1?"":"s");
        if(!confirm('Delete "'+project.name+'" and its linked '+linked+'? This cannot be undone unless you exported a backup.'))return;
        try{
          await apiSend("DELETE","/projects/"+encodeURIComponent(id));
          await refresh();
          linkedWorkIds.forEach(function(workId){delete expandedWorkIds[workId];});
          selectedId=data.projects[0]?data.projects[0].id:"";
          render();Argo.toast("Project and linked records deleted");
        }catch(error){console.error(error);Argo.toast("Could not delete the project \u2014 check the server connection");}
      }

      document.addEventListener("click",function(event){
        var button=event.target.closest("button");
        if(!button)return;
        var action=button.dataset.action;
        var id=button.dataset.id;
        if(action==="select-project"){selectedId=id;render();document.getElementById("detail").scrollIntoView({behavior:"smooth",block:"start"});}
        if(action==="edit-project")openProject(id);
        if(action==="delete-project")deleteProject(id);
        if(action==="add-work")openWork();
        if(action==="edit-work")openWork(id);
        if(action==="toggle-work"){expandedWorkIds[id]=!expandedWorkIds[id];renderDetail();}
        if(action==="add-activity")openActivity(button.dataset.workId);
        if(action==="edit-activity"){var activity=data.activities.find(function(item){return item.id===id;});if(activity)openActivity(activity.workItemId,id);}
        if(action==="add-raid")openRaid();
        if(action==="edit-raid")openRaid(id);
        if(button.dataset.close)document.getElementById(button.dataset.close).close();
      });
      document.getElementById("add-project-btn").addEventListener("click",function(){openProject();});
      document.getElementById("project-status").addEventListener("change",syncPriorityField);
      document.getElementById("board").addEventListener("change",function(event){if(event.target.dataset.action==="sort-waiting"){waitingSort=event.target.value;renderBoard();}});
      document.getElementById("project-form").addEventListener("submit",async function(event){
        event.preventDefault();
        var record=formObject(event.currentTarget);
        try{
          await apiSend("PUT","/projects/"+encodeURIComponent(record.id),record);
          await refresh();
          selectedId=record.id;
          document.getElementById("project-dialog").close();
          render();Argo.toast("Project saved");
        }catch(error){console.error(error);Argo.toast("Could not save the project \u2014 check the server connection");}
      });
      document.getElementById("work-form").addEventListener("submit",async function(event){
        event.preventDefault();
        var record=formObject(event.currentTarget);
        try{
          await apiSend("PUT","/workitems/"+encodeURIComponent(record.id),record);
          await refresh();
          expandedWorkIds[record.id]=true;
          document.getElementById("work-dialog").close();
          render();Argo.toast("Work package saved");
        }catch(error){console.error(error);Argo.toast("Could not save the work package \u2014 check the server connection");}
      });
      document.getElementById("activity-form").addEventListener("submit",async function(event){
        event.preventDefault();
        var record=formObject(event.currentTarget);
        try{
          await apiSend("PUT","/activities/"+encodeURIComponent(record.id),record);
          await refresh();
          expandedWorkIds[record.workItemId]=true;
          document.getElementById("activity-dialog").close();
          render();Argo.toast("Activity saved");
        }catch(error){console.error(error);Argo.toast("Could not save the activity \u2014 check the server connection");}
      });
      document.getElementById("raid-form").addEventListener("submit",async function(event){
        event.preventDefault();
        var record=formObject(event.currentTarget);
        try{
          await apiSend("PUT","/raid/"+encodeURIComponent(record.id),record);
          await refresh();
          document.getElementById("raid-dialog").close();
          render();Argo.toast("RAID record saved");
        }catch(error){console.error(error);Argo.toast("Could not save the RAID record \u2014 check the server connection");}
      });
      document.getElementById("export-btn").addEventListener("click",function(){var blob=new Blob([JSON.stringify(data,null,2)],{type:"application/json"});var url=URL.createObjectURL(blob);var link=document.createElement("a");link.href=url;link.download="argo-portfolio-backup-"+new Date().toISOString().slice(0,10)+".json";link.click();URL.revokeObjectURL(url);Argo.toast("Backup downloaded");});
      document.getElementById("import-btn").addEventListener("click",function(){document.getElementById("import-file").click();});
      document.getElementById("import-file").addEventListener("change",function(event){
        var file=event.target.files[0];if(!file)return;
        var reader=new FileReader();
        reader.onload=async function(){
          try{
            var restored=JSON.parse(String(reader.result));
            if(!Array.isArray(restored.projects)||!Array.isArray(restored.workItems)||!Array.isArray(restored.raidItems))throw new Error("Invalid");
            restored=normalize(restored);
            await apiSend("POST","/portfolio/import",restored);
            await refresh();
            selectedId=data.projects[0]?data.projects[0].id:"";
            expandedWorkIds={};
            render();Argo.toast("Backup imported to the shared Argo database");
          }catch(error){console.error(error);alert("That file is not a valid Argo Portfolio backup, or the server could not be reached.");}
        };
        reader.readAsText(file);
        event.target.value="";
      });

      async function init(){
        try{
          var ingestResult=await apiSend("POST","/portfolio/ingest");
          await refresh();
          selectedId=(ingestResult&&ingestResult.firstProjectId)||(data.projects[0]?data.projects[0].id:"");
          render();
          if(ingestResult&&ingestResult.count)Argo.toast(ingestResult.count+" submitted request"+(ingestResult.count===1?"":"s")+" added to Waiting");
        }catch(error){
          console.error(error);
          data=clone(cleanSeed);
          render();
          Argo.toast("Could not reach the Argo server. Check that the app is running.");
        }
      }
      init();
    })();
  