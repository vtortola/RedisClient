proc PaginationTest(zsetKey, page, itemsPerPage)
	
	local start  = page * itemsPerPage
	local stop = start + itemsPerPage - 1
	local items = redis.call('ZREVRANGE', zsetKey, start, stop)

	local result = {}

	for index, key in ipairs(items) do
	  result[index] = redis.call('HGETALL', key)
	end

	return result
endproc