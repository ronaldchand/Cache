# MemCache
A in memory cache that allows you to specify the maximum number of records to be stored in memory. Once the max number is records equals or exceeds the maximum allowed value, the oldest record is removed. 

## Usage
MemCache implements `MemCache<TKey, TValue>` which allows you to store any type of data you want. **Key cannot be null** 

To add data to cache:
```
  //the 2 is the maximum number being set here. You can set it a higher value depending on 
  //available memory
  var memCache = new MemCache<string,string>(2); 
 _memCache.AddOrUpdate("firstKey", "firstValue");
 ```
 Records are updated if the key is found. Keys are case sensitive 
 
 To retrieve a value from cache:
 
 ``` 
  if (memCache.TryGetValue("firstKey", out var cachedValue))
  {
      //do something with the value
  }
```
## Code Coverage
   Project   |    NotCovered(Blocks) |   Not Covered %     |   Covered(Blocks)     |   Covered % 
   ---       | ---                   |   ---               | ---                   | ---
  MemCache   |	  1	                 |   2.78%	           | 35	                   | 97.22%
