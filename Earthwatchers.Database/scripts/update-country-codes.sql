update Earthwatcher
  set country = case when country = 'Argentina' then 'AR'
					 when country = 'Bolivia' then 'BO'
					 when country = 'Chile' then 'CL'
					 when country = 'Colombia' then 'CO'
					 when country = 'Costa Rica' then 'CR'
					 when country = 'España' then 'ES'
					 when country = 'India' then 'IN'
					 when country = 'México' then 'MX'
					 when country = 'Países Bajos' then 'NL'
					 when country = 'Perú' then 'PE'
					 when country = 'Portugal' then 'PT'
					 when country = 'Turquía' then 'TR'
					 when country = 'United Kingdom' then 'GB'
					 when country = 'United States' then 'US'
					 when country = 'Uruguay' then 'UY'
					 else country
					 end