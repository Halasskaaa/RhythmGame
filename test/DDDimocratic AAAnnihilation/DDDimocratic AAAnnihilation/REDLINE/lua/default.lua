
checked = false
mod_plr = {}

FRAME_RATE = 60

-- EASING --
local path = GAMESTATE:GetCurrentSong():GetSongDir()..'lua/'
loadfile(path..'easing.lua')()
loadfile(path..'modhelpers.lua')()
loadfile(path.."mods.lua")()

local function Plr(pn)
	if GAMESTATE:IsPlayerEnabled(pn-1) then
		return SCREENMAN:GetTopScreen():GetChild('PlayerP'..pn)
	else
		return nil
	end
end
function GetJud(pn)
	return _G['jud'..pn]
end
function GetCom(pn)
	return _G['com'..pn]
end


function mod_do(str,pn)
	if pn then
		if GAMESTATE:IsPlayerEnabled(pn-1) then
			taronuke_mods(str,pn)
		end
	else
		for pn=1,2 do
			if GAMESTATE:IsPlayerEnabled(pn-1) then
				taronuke_mods(str,pn)
			end
		end
	end
end

-- ALIASES --
--These make it so the syntax for the table entries matches the syntax for the nITG template.
--If you want to prefix all your calls with "modhelpers.", "ease.", etc., then you won't need this and can remove this section.
local linear, inQuad, outQuad, inOutQuad, outInQuad, inCubic, outCubic, inOutCubic, outInCubic, inQuart,outQuart, inOutQuart, outInQuart, inQuint, outQuint, inOutQuint, outInQuint, inSine, outSine, inOutSine,outInSine, inExpo, outExpo, inOutExpo, outInExpo, inCirc, outCirc, inOutCirc, outInCirc, inElastic,outElastic, inOutElastic, outInElastic, inBack, outBack, inOutBack, outInBack, inBounce, outBounce,inOutBounce, outInBounce = ease.linear, ease.inQuad, ease.outQuad, ease.inOutQuad, ease.outInQuad,ease.inCubic, ease.outCubic, ease.inOutCubic, ease.outInCubic, ease.inQuart, ease.outQuart,ease.inOutQuart, ease.outInQuart, ease.inQuint, ease.outQuint, ease.inOutQuint, ease.outInQuint,ease.inSine, ease.outSine, ease.inOutSine, ease.outInSine, ease.inExpo, ease.outExpo, ease.inOutExpo,ease.outInExpo, ease.inCirc, ease.outCirc, ease.inOutCirc, ease.outInCirc, ease.inElastic,ease.outElastic, ease.inOutElastic, ease.outInElastic, ease.inBack, ease.outBack, ease.inOutBack,ease.outInBack, ease.inBounce, ease.outBounce, ease.inOutBounce, ease.outInBounce

local perframe, mod_insert, mod2_insert, mod_ease, mod_perframe, mod_message, mod_blacklist, simple_m0d, simple_m0d2, simple_m0d3, mod_wiggle, mod_spring, mod_springt, mod_springt2, mod_spring_adjustable, mod_onebeat, switcfino_add, switcfino_reset, mod_sugarkiller, modulo, reverseRotation, randomXD, mod_bounce, ease_wiggle, ease_wiggleAbs = modhelpers.perframe, modhelpers.mod_insert, modhelpers.mod2_insert, modhelpers.mod_ease, modhelpers.mod_perframe, modhelpers.mod_message, modhelpers.mod_blacklist, modhelpers.simple_m0d, modhelpers.simple_m0d2, modhelpers.simple_m0d3, modhelpers.mod_wiggle, modhelpers.mod_spring, modhelpers.mod_springt, modhelpers.mod_springt2, modhelpers.mod_spring_adjustable, modhelpers.mod_onebeat, modhelpers.switcfino_add, modhelpers.switcfino_reset, modhelpers.mod_sugarkiller, modhelpers.modulo,modhelpers.reverseRotation, modhelpers.randomXD, modhelpers.mod_bounce, modhelpers.ease_wiggle, modhelpers.ease_wiggleAbs

			local m = mod_insert
			local m2 = mod_message
			local me = mod_ease
			local mb = mod_bounce
			local m_bl = 60/140
			local l = 'len'
			local e = 'end'
				
			
			
			init_modsp1 = ''
			init_modsp2 = ''
			
			mod_plr = {}
			
			mod_firstSeenBeat = GAMESTATE:GetSongBeat() --necessary to prevent long freezes
			
			
			local poptions = {GAMESTATE:GetPlayerState(0):GetPlayerOptions('ModsLevel_Song'), GAMESTATE:GetPlayerState(1):GetPlayerOptions('ModsLevel_Song')}
							
			local isusingreverse = {false, false}
			for pn = 1, 2 do
				if poptions[pn] then
					isusingreverse[pn] = poptions[pn]:Reverse() ~= 0
				end
			end
---------------------------------------------------------------------------------------
----------------------Begin tables 'n stuff--------------------------------------------
---------------------------------------------------------------------------------------

			--beat based mods
			--{beat_start, beat_end, mods, len_or_end, player_number}
			mods = {
				{0,9999,'*1000 no beat, *1000 no drunk, *1000 no tipsy, *1000 no invert, *1000 no flip, *1000 no dizzy','end'},
			}
						
			--this is both a message broadcaster and a function runner
			--if you put {beat,'String'}, then 'String' is broadcast as a message on that beat
			--if you put {beat,function() somecode end}, then function() is run at that beat
			--see example on beat 32
			
			curaction = 1
			--{beat,thing,persists}
			mod_actions = {
			}
						
			--beat-based ease mods
			--{time_start, time_end, mod_start, mod_end, mod, len_or_end, ease_type, player_number, sustaintime, optional_param1, optional_param2}
			--valid ease types are:
				--linear
				--inQuad    outQuad    inOutQuad    outInQuad
				--inCubic   outCubic   inOutCubic   outInCubic
				--inQuart   outQuart   inOutQuart   outInQuart
				--inQuint   outQuint   inOutQuint   outInQuint
				--inSine    outSine    inOutSine    outInSine
				--inExpo    outExpo    inOutExpo    outInExpo
				--inCirc    outCirc    inOutCirc    outInCirc
				--inElastic outElastic inOutElastic outInElastic    --can take 2 optional parameters - amplitude & period
				--inBack    outBack    inOutBack    outInBack       --can take 1 optional parameter  - spring amount
				--inBounce  outBounce  inOutBounce  outInBounce
				
			mods_ease = {
				-- EXAMPLE TWEEN: from beat 0 to 4, make rotationz go from 360 to 0 with the inOutBack tween
				--{0,4,360,0,'rotationz','end',inOutBack},
			}
			
			
			
			mod_perframes = {
				
			}
			
			function mpf(s,e,f)
				table.insert(mod_perframes,{s,e,f})
			end
			
			local h = SCREEN_HEIGHT / 480
			
			for pn = 1,2 do
				if GAMESTATE:IsPlayerEnabled(pn-1) then
					if GAMESTATE:GetCurrentSteps(pn-1):GetDifficulty() == 'Difficulty_Challenge' or GAMESTATE:GetCurrentSteps(pn-1):GetDifficulty() == 'Difficulty_Hard' then
						-- forgive me sorae for what i yabba dabba did (stole this from and drugs)
						function bouncecol(b,c,h,len,pn)
							local split = (c==0 or c==1) and -h or h
							local cross = (c==0 or c==3) and -h or h 
							local alternate = (c==0 or c==2) and -h or h
							local reverse = (c==0) and 2*h or 0
							me(b,len,0,split,'split',l,outCubic, pn)
							me(b+len,len,split,0,'split',l,inCubic, pn)
							me(b,len,0,cross,'cross',l,outCubic, pn)
							me(b+len,len,cross,0,'cross',l,inCubic, pn)
							me(b,len,0,alternate,'alternate',l,outCubic, pn)
							me(b+len,len,alternate,0,'alternate',l,inCubic, pn)
							me(b,len,0,reverse,'reverse',l,outCubic, pn)
							me(b+len,len,reverse,0,'reverse',l,inCubic, pn)
							m(b+len*2,0.25,'*100 no reverse, *100 no split, *100 no cross, *100 no alternate',l,pn)
						end
						function bouncecolreversed(b,c,h,len,pn)
							local split = (c==0 or c==1) and -h or h
							local cross = (c==0 or c==3) and -h or h
							local alternate = (c==0 or c==2) and -h or h
							local reverse = (c==0) and 2*h or 0
							me(b,len,0,split,'split',l,outCubic, pn)
							me(b+len,len,split,0,'split',l,inCubic, pn)
							me(b,len,0,cross,'cross',l,outCubic, pn)
							me(b+len,len,cross,0,'cross',l,inCubic, pn)
							me(b,len,0,alternate,'alternate',l,outCubic, pn)
							me(b+len,len,alternate,0,'alternate',l,inCubic, pn)
							me(b,len,100,100+reverse,'reverse',l,outCubic, pn)
							me(b+len,len,100+reverse,100,'reverse',l,inCubic, pn)
							m(b+len*2,0.25,'*100 100 reverse, *100 no split, *100 no cross, *100 no alternate',l,pn)
						end

						-- beat mod every other beat from (start, end)
						bigBeatStretches = 
						{
							{33, 47},
							{57, 81},
							{197, 223},
							{311, 337},
							{347, 373},
							{411, 481}
						}

						for i, v in ipairs(bigBeatStretches) do
							for j = v[1],v[2],4 do
								simple_m0d(j, 100, 0.25, 'beat', pn)
							end
							for j = v[1]+2,v[2],4 do
								simple_m0d(j, -100, 0.25, 'beat', pn)
							end
						end

						-- flash every eight beats 106 - 170
						for i = 106,170,8 do
							simple_m0d(i, 50, 0.2, 'stealth', pn)
						end
 
						--bespoke stealthing
						m(49, 0.25, '50% stealth', 'len', pn)
						m(53, 1, '0% stealth', 'len', pn)
						me(80, 3.5, 0, 100, 'stealth', 'len', linear, pn)
						me(87, 1, 100, 0, 'stealth', 'len', linear, pn)
						me(338, 4, 0, 100, 'stealth', 'len', linear, pn)
						me(342, 2, 100, 0, 'stealth', 'len', linear, pn)
						-- me(87, 2, '0 stealth', 'len', pn)
						-- simple_m0d(30, 150, 0.25, 'beat', pn)
						--	beat	len		(start/end)	mod			len (ease func) pn
						-- mb(16, 1, 0, -12.5, 'Alternate', outSine, inSine, pn)

						shortDrunks = 
						{
							{378.0, 50}, 
							{379.5, 50}, 
							{382.0, 50}, 
							{383.5, 50}, 
							{386.0, 50},
							{387.5, 50},
							{390.0, 50},
							{394.0, 50},
							{395.5, 50},
							{398.0, 50},
							{399.5, 50},
							{402.0, 50},
							{403.5, 50},
							{406.0, 50},
							{442, 50},
							{443.5, 50},
							{446, 50},
							{447.5, 50},
							{450, 50},
							{451.5, 50},
							{454, 50},
							{458, 50}, 
							{459.5, 50}, 
							{462, 50}, 
							{463.5, 50}, 
							{466, 50}, 
							{467.5, 50}, 
							{470, 50},
						}

						for i, v in ipairs(shortDrunks) do
							simple_m0d(v[1], v[2], 0.2, 'tipsy', pn)
							simple_m0d(v[1], v[2]/3, 0.2, 'drunk', pn)
						end

						-- extended tipsy/drunks
						mb(391.5, 2.5, 0, 35, 'tipsy', outCubic, inCubic, pn)
						mb(407.5, 2.5, 0, 35, 'tipsy', outCubic, inCubic, pn)
						mb(455.5, 2.5, 0, 35, 'tipsy', outCubic, inCubic, pn)
						mb(471.5, 2.5, 0, 35, 'tipsy', outCubic, inCubic, pn)

						mb(391.5, 2.5, 0, 10, 'drunk', outCubic, inCubic, pn)
						mb(407.5, 2.5, 0, 10, 'drunk', outCubic, inCubic, pn)
						mb(455.5, 2.5, 0, 10, 'drunk', outCubic, inCubic, pn)
						mb(471.5, 2.5, 0, 10, 'drunk', outCubic, inCubic, pn)

						longBounces =
						{
							{228, 0, 3.5},
							{231.5, 3, 4.5},
							{236, 2, 3.5},
							{239.5, 1, 4},
							{244, 0, 3.5},
							{247.5, 3, 4.5},
							{252, 2, 3.5},
							{255.5, 1, 2},
							{258, 0, 0.5},
							{258.5, 2, 0.5},
						}

						for i, v in ipairs(longBounces) do
							if (isusingreverse[pn]) then
								bouncecolreversed(v[1], v[2], 3, v[3]/2.0, pn)
							else
								bouncecol(v[1], v[2], -3, v[3]/2.0, pn)
							end
							if (v[3] > 0.5) then
								mb(v[1], v[3], 0, 10, 'flip', outCubic, inCubic)
							end
						end

						if (isusingreverse[pn]) then
							bouncecolreversed(259.5, 3, 3, 5.25, pn)
						else
							bouncecol(259.5, 3, -3, 5.25, pn)
						end
						mb(259.5, 10.5, 0, 10, 'flip', outCubic, inCubic)




						-- beat, column to bounce at beat
						bigBounces = 
						{
							{410.5, 0},
							{414.5, 0},
							{418.5, 0},
							{422.5, 0},
							{426.5, 3},
							{430.5, 3},
							{434.5, 3},
							{438.5, 3},
							{442.5, 3},
							{446.5, 3},
							{450.5, 0},
							{454.5, 0},
							{458.5, 0},
							{462.5, 0},
							{466.5, 3},
							{470.5, 3},
							{474.5, 0},
							{478.5, 0},
						}

						for i, v in ipairs(bigBounces) do
							if (isusingreverse[pn]) then
								bouncecolreversed(v[1], v[2], 3, 0.5, pn)
							else
								bouncecol(v[1], v[2], -3, 0.5, pn)
							end
							mb(v[1], 1, 0, 10, 'flip', outCubic, inCubic)
						end


						-- i didn't like these very much but try uncommenting them for extra effects i guess
						-- -- beat, column(s) to bounce every 0.5 beats
						-- smallBounces = 
						-- {
						-- 	{411.5, 2, 3, 1, 2},
						-- 	{415.5, 1, 3, 2, 1},
						-- 	{419.5, 2, 3, 1, 2},
						-- 	{423.5, 3, 2, 3, 1},
						-- 	{427.5, 1, 0, 2, 1},
						-- 	{431.5, 2, 0, 1, 2},
						-- 	{435.5, 1, 0, 2, 1},
						-- 	{439.5, 2, 1, 2, 0},
						-- 	{443.5, 2, 0, 2, 1},
						-- 	{447.5, 1, 0, 1, 2},
						-- 	{451.5, 1, 3, 1, 2},
						-- 	{455.5, 2, 2, 1, 2},
						-- 	{459.5, 2, 3, 2, 1},
						-- 	{463.5, 1, 3, 1, 2},
						-- 	{467.5, 1, 0, 1, 2},
						-- 	{471.5, 2, 2, 0, 1},
						-- 	{475.5, 0, 2, 3, 1},
						-- 	{479.5, 0, 1, 3, 1},
						-- }
								
						-- for i, v in ipairs(smallBounces) do
						-- 	if (isusingreverse[pn]) then
						-- 		bouncecolreversed(v[1], v[2], 2, 0.25, pn)
						-- 		bouncecolreversed(v[1]+0.5, v[3], 2, 0.25, pn)
						-- 		bouncecolreversed(v[1]+1.0, v[4], 2, 0.25, pn)
						-- 		bouncecolreversed(v[1]+1.5, v[5], 2, 0.25, pn)
						-- 	else
						-- 		bouncecol(v[1], v[2], -2, 0.25, pn)
						-- 		bouncecol(v[1]+0.5, v[3], -2, 0.25, pn)
						-- 		bouncecol(v[1]+1.0, v[4], -2, 0.25, pn)
						-- 		bouncecol(v[1]+1.5, v[5], -2, 0.25, pn)
						-- 	end
						-- end

					end
				end
			end
			
			
			
			function modtable_compare(a,b)
				return a[1] < b[1]
			end
			
			if table.getn(mod_actions) > 1 then
				table.sort(mod_actions, modtable_compare)
			end

local ac = Def.ActorFrame{
	Name= "xtl_actor_d",
	Def.Actor{ OnCommand= function(s) mod_firstSeenBeat = GAMESTATE:GetSongBeat() s:sleep(9e9) end },
	Def.Actor{ InitCommand= function(s) my_auxvar = s end,
	},
	Def.Actor{
		
		OnCommand=function(s)
			s:queuecommand('Update')
		end,
		UpdateCommand=function(self)
			
			if GAMESTATE:GetSongBeat()>=0 and not checked then
			
				--name players, judgment and combo
				for pn=1,2 do
					_G['P'..pn] = SCREENMAN:GetTopScreen():GetChild('PlayerP'..pn) or nil
					if _G['P'..pn] then
						_G['jud'..pn] = _G['P'..pn]:GetChild('Judgment')
						_G['com'..pn] = _G['P'..pn]:GetChild('Combo')
					end
					
				end
				
				screen = SCREENMAN:GetTopScreen()
				checked = true --let this only run once
				
			end

			local beat = GAMESTATE:GetSongBeat()
			
			
---------------------------------------------------------------------------------------
----------------------Begin table handlers---------------------------------------------
---------------------------------------------------------------------------------------
			
			disable = false
			if disable ~= true and beat > mod_firstSeenBeat+0.1 and checked then
				
				-----------------------
				-- Player mod resets --
				-----------------------
				for i=1,2 do
					--mod_do('clearall',i)
				end
				
				------------------------------------------------------------------------------
				-- custom mod reader by TaroNuke edited by WinDEU and re-stolen by TaroNuke --
				------------------------------------------------------------------------------
				for i,v in pairs(mods) do
					if v and table.getn(v) > 3 and v[1] and v[2] and v[3] and v[4] then
						if beat >=v[1] then
							if (v[4] == 'len' and beat <=v[1]+v[2]) or (v[4] == 'end' and beat <=v[2]) then
								if table.getn(v) == 5 then
									mod_do(v[3],v[5])
								else
									mod_do(v[3])
								end
							end
						end
					else
						v[1] = 0
						v[2] = 0
						v[3] = ''
						v[4] = 'error'
						SCREENMAN:SystemMessage('Bad mod in beat-based table (line '..i..')')
					end
				end
				
				--------------------------------------------------------------------------------------
				-- i dont know who this reader is but he looks like he is made out of EASE HAHAHAHA --
				-- original code by exschwasion, bastardized by taro for cmod support and less 'if' --
				--------------------------------------------------------------------------------------
				for i,v in pairs(mods_ease) do
					if v and table.getn(v) > 6 and v[1] and v[2] and v[3] and v[4] and v[5] and v[6] and v[7] then
						if beat >=v[1] then
							if (v[6] == 'len' and beat <=v[1]+v[2]) or (v[6] == 'end' and beat <=v[2]) then
								
								local duration = v[2]
								if v[6] == 'end' then duration = v[2] - v[1] end
								local curtime = beat - v[1]
								local diff = v[4] - v[3]
								local startstrength = v[3]
								local curve = v[7]
								local mod = v[5]
								
								local strength = curve(curtime, startstrength, diff, duration, v[10], v[11]) --extra parameters for back and elastic eases :eyes:
								
								if type(mod) == 'function' then
									mod(strength)
								else
									local modstr = v[5] == 'xmod' and strength..'x' or (v[5] == 'cmod' and 'C'..strength or strength..' '..v[5])
									mod_do('*10000 '..modstr,v[8]);
								end
								
							elseif (v[9] and ((v[6] == 'len' and beat <=v[1]+v[2]+v[9]) or (v[6] == 'end' and beat <=v[9]))) then
							
								local strength = v[4]
								if type(mod) == 'function' then
									mod(strength)
								else
									local modstr = v[5] == 'xmod' and strength..'x' or (v[5] == 'cmod' and 'C'..strength or strength..' '..v[5])
									mod_do('*10000 '..modstr,v[8]);
								end
								
							end
						end
					else
						SCREENMAN:SystemMessage('Bad mod in beat-based ease table (line '..i..')')
					end
				end
				
				--------------------
				-- Perframe stuff --
				--------------------
				
				----------------------------------------
				-- HBLBHCBLBJGBHL DO THIS EVERY FRAME --
				----------------------------------------
				
				if table.getn(mod_perframes)>0 then
					for i=1,table.getn(mod_perframes) do
						local a = mod_perframes[i]
						if beat > a[1] and beat < a[2] then
							a[3](beat)
						end
					end
				end
				
				---------------------------------------
				-- ACTION RPGS AINT GOT SHIT ON THIS --
				---------------------------------------
				while curaction<=table.getn(mod_actions) and GAMESTATE:GetSongBeat()>=mod_actions[curaction][1] do
					if mod_actions[curaction][3] or GAMESTATE:GetSongBeat() < mod_actions[curaction][1]+2 then
						if type(mod_actions[curaction][2]) == 'function' then
							mod_actions[curaction][2]()
						elseif type(mod_actions[curaction][2]) == 'string' then
							MESSAGEMAN:Broadcast(mod_actions[curaction][2])
						end
					end
					curaction = curaction+1
				end
				
			end
			
			self:sleep(1/FRAME_RATE)
			self:queuecommand('Update')

---------------------------------------------------------------------------------------
----------------------END DON'T TOUCH IT KIDDO-----------------------------------------
---------------------------------------------------------------------------------------
			
		end,

	},

}

return ac