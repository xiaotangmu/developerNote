Java面试

1. 泛型

参数化类型

2. 线程



3.请你说明一下ConcurrentHashMap的原理？

jdk1.7中采用Segment + HashEntry的方式进行实现 1.8中是采用Node + CAS + Synchronized来保证并发安全进行实现.



ConcurrentHashMap 类中包含两个静态内部类 HashEntry 和 Segment。HashEntry 用来封装映射表的键 / 
值对；Segment 用来充当锁的角色，每个 Segment 对象守护整个散列映射表的若干个桶。每个桶是由若干个 HashEntry 
对象链接起来的链表。一个 ConcurrentHashMap 实例中包含由若干个 Segment 对象组成的数组。HashEntry 
用来封装散列映射表中的键值对。



  首先先说JDK1.7版本，1.7的思路是分段锁。啥意思呢？ 

  对于共享资源，频繁地多线程会造成线程上下文地切换，怎么尽量避免这个问题了，这边就引入了锁的粒度这个概念，比如就以数组为例，我可以切分数组把一个大数组分成16段，如果多线程分别操作的是不同的段那么就不存在多线程竞争关系也相对线程安全，这就是分段锁。 

  你可以把1.7的ConcurrentHashMap想象成是16个线程安全的hashtable拼凑成的hashMap。每次新增一个Key的时候，我们会进行二阶段hash，第一阶段hash定位到这个key属于哪个分段，第二阶段hash把这个key定位到这个分段数组的什么下标，采取这种方式就是它的原理。学习concurrenthashmap我们主要是要观察他的如下几个方法：put，get，remove，resize，size。这边就不多言了，内容比较多，建议大家去看下源码吧，这边注意的一点是get是不加锁的，具体原因可以思考一下【用到了某个关键字】。 

  1.8的话抛弃了分段锁的概念，而是采取了cas和synchronized来保证并发安全，synchronized只锁住当前链表或者红黑二叉树的首节点，只要hash不冲突，就不会产生并发，效率很高。 



4. 类与对象

类：抽象的

对象：实实在在的个体



.类是对某一类事物的描述，是抽象的；而对象是一个实实在在的个体，是类的一个实例。



5. 斐波那契数列

1、1、2、3、5、8、13、21、34、……

*F*(1)=1，*F*(2)=1, *F*(n)=*F*(n - 1)+*F*(n - 2)（n ≥ 3）





