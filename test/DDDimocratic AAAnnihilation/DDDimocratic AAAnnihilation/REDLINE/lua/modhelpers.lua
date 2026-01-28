---------------------------------------------------------------------------------------
----------------------DON'T TOUCH IT KIDDO---------------------------------------------
---------------------------------------------------------------------------------------
LoadActor('easing.lua')
modhelpers = {}
modhelpers.perframe = function( beat, endBeat )
	if GAMESTATE:GetSongBeat() < mod_firstSeenBeat+1 then return false end
	if not endBeat then endBeat = beat+1 end
	local cur = GAMESTATE:GetSongBeat();
	if cur>=beat and cur<endBeat then return true end
	return false
end

modhelpers.mod_insert = function(beat,len,mod,t,pn)
	if not t then t = 'len' end
	table.insert(mods,{beat,len,mod,t,pn});
end
modhelpers.mod_ease = function(beat,len,str1,str2,mod,t,ease,pn,sus,opt1,opt2)
	table.insert(mods_ease,{beat,len,str1,str2,mod,t,ease,pn,sus,opt1,opt2});
end
modhelpers.mod_perframe = function(start_beat,end_beat,f)
	table.insert(mod_perframes,{start_beat,end_beat,f});
end
modhelpers.mod_message = function(beat,msg,p)
	table.insert(mod_actions,{beat,msg,p});
end

modhelpers.mod_blacklist = function(t,n)
	for i=1,table.getn(t) do
		if t[i] == n then
			return true
		end
	end
	return false
end

--how can I make simple m0d?
--this will instantly turn a mod on to max and then quickly turn it off with the specified parameters
--it's a simple mod

modhelpers.simple_m0d = function(beat,strength,mult,mod,pn)
	if not strength then strength = 400 end
	if not mult then mult = 1 end
	if not mod then mod = 'drunk' end
	
	local alive = math.max(2*mult*math.abs(strength)/100,0.25)
	
	table.insert(mods,{beat,0.3,'*100000 '..strength..' '..mod,'len',pn});
	table.insert(mods,{beat+.2,alive,'*'..((1/mult)*math.abs(strength)/100)..' no '..mod,'len',pn});
end

modhelpers.simple_m0d2 = function(beat,strength,mult,mod,pn)
	if not strength then strength = 400 end
	if not mult then mult = 1 end
	if not mod then mod = 'drunk' end
	
	local alive = math.max(10*mult*math.abs(strength)/100,0.25)
	
	table.insert(mods,{beat,0.3,'*'..math.abs(strength/10)..' '..strength..' '..mod,'len',pn});
	table.insert(mods,{beat+.3,alive,'*'..((1/mult)*math.abs(strength)/100)..' no '..mod,'len',pn});
end

modhelpers.simple_m0d3 = function(beat,strength,duration,bpm,mod,pn)
	if not strength then strength = 400 end
	if not duration then duration = 1 end
	if not bpm then bpm = 120 end
	if not mod then mod = 'drunk' end
	
	local alive = duration * (60/bpm)
	local str = (1/(duration * 60/bpm)) * (math.abs(strength)/100)
	
	table.insert(mods,{beat-duration,duration,'*'..str..' '..strength..' '..mod,'len',pn});
	table.insert(mods,{beat,duration*1.1,'*'..str..' no '..mod,'len',pn});
end

--alternates a mod back and forth before resetting to 0
--beat,num,div,amt,speed,mod,pn
modhelpers.mod_wiggle = function(beat,num,div,amt,speed,mod,pn,first)
	if not speed then speed = 1 end
	local fluct = 1
	for i=0,(num-1) do
		b = beat+(i/div)
		local m = 1
		if i==0 and not first then m = 0.5 end
		table.insert(mods,{b,1,'*'..math.abs(m*speed*amt/10)..' '..(amt*fluct)..' '..mod..'','len',pn});
		fluct = fluct*-1;
	end
	table.insert(mods,{beat+(num/div),1,'*'..math.abs(amt*speed/20)..' no '..mod..'','len',pn});
end

--like wiggle, but springier
--beat,strength,num,mod,pn
modhelpers.mod_spring = function(beat,strength,num,mod,pn)
	local fluct = 1;
	for i=0,num do
		if i==0 then mult = 0.5 else mult = 1 end
		local amt = (num-i)/num
		local b = beat+(0.05*i)
		
		table.insert(mods,{b,0.3,'*'..math.max(math.abs(1000*strength*mult*amt),1)..' '..(strength*amt*fluct)..' '..mod,'len',pn});
		
		fluct = fluct*-1;
	end
end

modhelpers.mod_springt = function(beat,strength,dur,mod,pn)
	local fluct = 1;
	dur = math.max(dur,0.02)
	for i=0,dur-.01,0.02 do
		local mult = 1;
		local amt = (dur-i)/dur
		local b = beat+i
		
		table.insert(mods2,{b,0.02,'*100000 '..(strength*amt*fluct)..' '..mod,'len',pn});
		
		fluct = fluct*-1;
	end
	table.insert(mods2,{beat+dur,0.1,'*100000 no '..mod,'len',pn});
end

modhelpers.mod_springt2 = function(beat,strength,dur,mod,pn)
	local fluct = 1;
	for i=0,dur-.01,0.02 do
		local b = beat+i
		
		table.insert(mods2,{b,0.02,'*100000 '..(strength*fluct)..' '..mod,'len',pn});
		
		fluct = fluct*-1;
	end
	table.insert(mods2,{beat+dur,0.1,'*100000 no '..mod,'len',pn});
end

--beat,strength,num,period,mod,pn
modhelpers.mod_spring_adjustable = function(beat,strength,num,period,mod,pn, first)
	local fluct = 1;
	for i=0,num do
		if i==0 then mult = 0.5 else mult = 1 end
		local amt = (num-i)/num
		local b = beat+(period*0.5*i)
		
		local m = 1
		if i==0 and not first then m = 0.5 end
		
		table.insert(mods,{b,0.3,'*'..math.max(m*math.abs((0.2/period)*strength*mult*amt),1)..' '..(strength*amt*fluct)..' '..mod,'len',pn});
		
		fluct = fluct*-1;
	end
end

--turns on beat for the beat specified, at the desired strength	
modhelpers.mod_onebeat = function(beat,strength,pn)
	if not strength then strength = 1000 end;
	table.insert(mods,{beat-.5,1,'*10000 '..strength..' beat','len',pn});
	table.insert(mods,{beat+.5,0.25,'*10000 no beat','len',pn});
end

local switcheroo_flip = {0,0,0};
local switcheroo_invert = {0,0,0};

local switcheroo_width = 1;

local switcheroos = {normal = {0,0}, ldur = {0,0}, reset = {0,0},
	flip = {100,0}, rudl = {100,0}, invert = {0,100}, dlru = {0,100},
	ludr = {25,-75}, rdul = {75,75}, drlu = {25,125}, ulrd = {75,-125}, urld = {100,-100}}
modhelpers.switcheroo_add = function(beat,which,speed,len,pn)

	if not speed then speed = 1000000 end
	
	local mpn = 3
	if pn then mpn = pn end
	
	local w = {0,0}
	
	if type(which) == 'string' then w = switcheroos[which] end
	if type(which) == 'table' then w = which end
	
	if w then
		local targf = (switcheroos[which][1]*switcheroo_width) + ( 50 - switcheroo_width*50 )
		local targi = (switcheroos[which][2]*switcheroo_width)
		local sw_modlist = ''
		if switcheroo_flip[mpn] ~= targf then
			sw_modlist = sw_modlist..'*'..(0.01*speed*math.abs(targf-switcheroo_flip[mpn]))..' '..(targf)..' flip,'
		else
			sw_modlist = sw_modlist..'*1 '..(targf)..' flip,'
		end
		if switcheroo_invert[mpn] ~= targi then
			sw_modlist = sw_modlist..'*'..(0.01*speed*math.abs(targi-switcheroo_invert[mpn]))..' '..(targi)..' invert'
		else
			sw_modlist = sw_modlist..'*1 '..(targi)..' invert'
		end
		table.insert(mods,{beat,len,sw_modlist,'len',pn});
		
		--Trace('SPEED: '..(0.01*speed*math.abs(targf-switcheroo_flip)));
		--Trace(sw_modlist);
		
		if mpn == 3 then
			for apn=1,3 do
				switcheroo_flip[apn] = targf;
				switcheroo_invert[apn] = targi;
			end
		else
			switcheroo_flip[mpn] = targf;
			switcheroo_invert[mpn] = targi;
		end
	
	end
	
end

modhelpers.switcheroo_reset = function()
	switcheroo_flip = {0,0,0};
	switcheroo_invert = {0,0,0};
end

--portable version of the effect from sugarkill X
modhelpers.mod_sugarkiller = function(beat,duration,speed,minstealth,maxstealth,pn)
	if not minstealth then minstealth = 50 end
	if not maxstealth then maxstealth = 85 end
	if not speed then speed = 1 end
	local dur = duration
	if not dur then dur = 1 end
	dur = dur*speed;
	for i=0,math.max(dur-1,0) do
		table.insert(mods,{beat+(i*0.5),.25/speed,'*10000 Invert, *10000 no flip, *10000 '..maxstealth..'% Stealth','len',pn})
		table.insert(mods,{beat+(i*0.5)+.25/speed,.25/speed,'*10000 Flip, *10000 no invert, *10000 '..maxstealth..'% Stealth','len',pn})
		table.insert(mods,{beat+(i*0.5)+.50/speed,.25/speed,'*10000 Flip,*10000 -100% Invert,*10000 '..maxstealth..'% Stealth','len',pn})
		if i == math.max(dur-1,0) then
			table.insert(mods,{beat+(i*0.5)+.75/speed,.25/speed,'*10000 No Flip,*10000 No Invert,*10000 no Stealth','len',pn})
		else
			table.insert(mods,{beat+(i*0.5)+.75/speed,.25/speed,'*10000 No Flip,*10000 No Invert,*10000 '..minstealth..'% Stealth','len',pn})
		end
	end
end

-- hey alright
modhelpers.modulo = function(a, b)
	return a - math.floor(a/b)*b;
end

-- counterrotation function by BrotherMojo
-- returns confusion(x/y/z)offsets to counteract the given rotations
modhelpers.reverseRotation = function(angleX, angleY, angleZ)
	local DEG_TO_RAD = math.pi / 180
	angleX, angleY, angleZ = DEG_TO_RAD * angleX, DEG_TO_RAD * angleY, DEG_TO_RAD * angleZ
	local sinX = math.sin(angleX);
	local cosX = math.cos(angleX);
	local sinY = math.sin(angleY);
	local cosY = math.cos(angleY);
	local sinZ = math.sin(angleZ);
	local cosZ = math.cos(angleZ);
	return 100 * math.atan2(-cosX*sinY*sinZ-sinX*cosZ,cosX*cosY), 100 * math.asin(-cosX*sinY*cosZ+sinX*sinZ), 100 * math.atan2(-sinX*sinY*cosZ-cosX*sinZ,cosY*cosZ)
end

--dumb random function
modhelpers.randomXD = function(t)
	if t == 0 then return 0.5 else
	return modulo(math.sin(t * 3229.3) * 43758.5453, 1) end
end

-- a good function
modhelpers.mod_bounce = function(beat,length,start,apex,mod,ease1,ease2,pn,opt1)
	modhelpers.mod_ease(beat, (length/2), start, apex, mod, 'len', ease1,pn,nil,opt1)
	modhelpers.mod_ease(beat+(length/2), (length/2), apex, start, mod, 'len',ease2,pn,nil,opt1)
end

--[[local function func_bounce(beat,length,start,apex,func,ease)
	func_ease(beat,(length/2),start,apex,func,'len',ease['out'..tostring(ease)])
	func_ease(beat+(length/2),(length/2),apex,start,func,'len',ease['in'..tostring(ease)],nil,0.2)
end]]

--stolen from hal thank you very cool
modhelpers.ease_wiggle = function(beatS,beatE,amt,inc,str,ease,len,pn)
	local rotStart = 0
	local prevRot = amt
	local curRot = -amt
	local measure = tostring(len)
	local asdf = 0
	if measure == 'end' then
		asdf = beatE
	elseif measure == 'len' then
		asdf = (beatS+beatE)
	end
	for i = beatS,asdf,inc do
		if i == beatS then
			rotStart = 0
		else
			rotStart = prevRot
		end
		if i == asdf then
			rotEnd = 0
		else
			rotEnd = curRot
		end
			modhelpers.mod_ease(i-inc, inc, rotStart, rotEnd, str, 'len', ease, pn)
			prevRot = prevRot *-1
curRot = curRot *-1
	end
end

modhelpers.ease_wiggleAbs = function(beatS,beatE,amt,inc,str,ease,len,pn)
	local rotStart = 0
	local prevRot = 0
	local curRot = amt
	local counter = 1
	local measure = tostring(len)
	local asdf = 0
	if measure == 'end' then
		asdf = beatE
	elseif measure == 'len' then
		asdf = (beatS+beatE)
	end
	for i = beatS,asdf-inc,inc do
		if counter == -1 then
			preRot,curRot = amt, 0
		elseif counter == 1 then
			preRot,curRot = 0, amt
		end
		modhelpers.mod_ease(i, inc, preRot, curRot, str, 'len', ease, pn)
		counter = counter*-1
	end
end

return modhelpers
---------------------------------------------------------------------------------------
----------------------END DON'T TOUCH IT KIDDO-----------------------------------------
---------------------------------------------------------------------------------------
