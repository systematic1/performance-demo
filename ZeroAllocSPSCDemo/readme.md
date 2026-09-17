# C# High Performance Demo

### Single Producer Single Consumer

This C# application demonstrates how a producer application (i.e. one of the thread workers) and a consumer application (the other thread)
work together to receive a buffered set of product-order items using a shared in-memory storage queue employing a FIFO access method.

The implementation has a very large pre-allocated buffer array (queue) allowing concurrent access to the shared queue by the producer
and the consumer running simultaneously. The large memory allocation is to avoid locking if the producer pushes too many in the queue
before the consumer can remove them.

* Lock free ring queue for speed with fixed initial preallocated ArrayPool buffer to avoid GC running
* Concurrent access by two separate threads
* Basic low latency demonstration with .NET Core 10 and C#
