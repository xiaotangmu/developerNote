# .Net能干什么

1 桌面应用程序 

Winform （.net 开发的桌面应用程序叫Winform 应用程序）

2 Internet 应用程序

ASP.NET

3 手机开发

wp8

4 Unity3D游戏开发或者虚拟现实

# .NET两种交互模式

**C/S**: 客户机（Client）/服务器模式（Server）

如：qq

C/S 是Client/Server 的缩写，客户端需要安装专用的客户端软件。

winform 应用程序

**B/S**: 浏览器（Browser）/服务器模式（Server）

B/S 是Brower/Server 的缩写，客户机只要安装一个浏览器

Internet 应用程序

# C#

1. 解决方案 -- f6 运行，可以检查代码是否有语法错误
2. 项目运行 f5 （调试），不调试ctrl + f5

解决方案下项目的卸载和加载 

## 二、注释的使用

解释

注销

1）单行注释 //

2）多行注释 /* */

3）文档注释 /// 多用来解释类或方法

## 三、VS 中的常用快捷键

1）Ctrl + K + D 快速对其代码

2）Ctrl + J 快速弹出智能提示

3）Ctrl + K + C 注释所选代码

4）Ctrl + K + U 取消对所选代码的注释

5）F1 转到帮助文档

6）折叠冗余代码 #Region 和 #EngRegion

## 四、变量

int num; 声明

num = 4; 赋值，将值放入声明空间

金钱类型 decimal  后面 + m

decimal d = 23.33m

先声明再赋值再使用

### 波浪线

​	红色波浪线 ：有语法错误

​	绿色波浪线：没有错误，可能会出现错误

### 命名规则

Camel：除第一个单词外，首字母大小，totalNum

Pascal：首字母都大小 MainName

### 占位符

Console.WriteLine("Hello {1}, are you want {2}", n1, n2);

多填后面参数，没效果，少填了，出现异常

异常: 没有语法错误，运行期间由于某些原因出现问题，导致程序不能正常运行。

### 接收输入

string str = Console.ReadLine();

### 转义符

\ + 特殊字符

### 字符串格式化

Console.WriteLine("{0, 0.00000}", 33);

类似 “{0, F2}”, 33 => 33.00

## 类型转换

自动类型转换, 小转大

int i = 4;

double d = i;

显示转换（强制转换），不声明会直接报错，声明丢弃多余位数据

double d = 3.3;

int i = (int) d;

字符串转换位其他类型

(1) int i = Convert.ToInt32(str);

double d = Convert.ToDouble(str);

...



(2) int number = int.Parse("123");

(3) bool b = int.TryParse("123", out number);

### Switch

switch ()

{

​	case case1: 执行代码;

​	break;

​	

}







# Out、ref、params

1）out 参数

如果你在一个方法中，返回多个相同类型的值的时候，返回数组就不行了，那么这个时候，我们可以考虑使用out 参数

out 参数就侧重与在一个方法中可以返回多个不同类类型的值。



