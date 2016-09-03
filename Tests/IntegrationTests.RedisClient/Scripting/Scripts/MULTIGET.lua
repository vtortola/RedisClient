-- http://beforeitwasround.com/2014/07/using-lua-to-implement-multi-get-on-redis-hashes.html

proc multiget($keys[])
	local result = {}

	for index, key in ipairs(keys) do
	  result[index] = redis.call('HGETALL', key)
	end

	return result
endproc