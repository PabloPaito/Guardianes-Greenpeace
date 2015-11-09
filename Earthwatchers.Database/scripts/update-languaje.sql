update Earthwatcher 
set Language =  case when Language = 'Spanish' then 'es-AR'
					 when Language = 'English' then 'en-US'
end