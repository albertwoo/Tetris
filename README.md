这是一个学习项目，以下内容为学习笔记。

# 加减数字示例
1. Redux里的Store对应Model，目前只有数字x；
2. Redux里的Action对应Msg，目前可以对状态进行加或减；
3. Redux里的初始化对应init，目前x设为0，并发一个Msg说要进行加操作，所以界面应该显示为1；
4. Redux里的Reducer对应update，这是唯一一个能对状态进行更改的地方，而且fsharp本身默认都是immutable的，所以你也无法对状态在其他任何地方进行更改；在redux里会用immutablejs来做，但是个人使用发现挺麻烦。
5. view里面就是基于状态model进行渲染，用户的点击等操作通过发布Msg来通知Program调用update来修改状态；
6. Program就是把init, update, view绑定在一起然后跑起来。
```fsharp
    type Model = { x : int }

    type Msg = Increment | Decrement

    let init () = 
        { x = 0 }, Cmd.ofMsg Increment

    let update msg model =
        match msg with
        | Increment -> { model with x = model.x + 1 }, Cmd.none
        | Decrement -> { model with x = model.x - 1 }, Cmd.none

    let view model dispatch =
        div []
            [
                div [] [str (string model.x)]
                button [OnClick (fun e -> dispatch Increment)] [str "+" ]
                button [OnClick (fun e -> dispatch Decrement)] [str "-" ]
            ]

    Program.mkProgram init update view 
    |> Program.run
```
整个程序真的是太简洁易懂了，可能会有一些语法需要习惯，比如`type Msg = Increment | Decrement`中的`|`相当于就是`或`，而`match msg with`类似`switch case`，但是功能强大很多。view里面的`div [放HTML标签属性] [放子元素]`。整个程序几乎每一行都是在写自己的业务，冗余的内容很少，真是太优雅了。**忍不住要把自己的第一篇文章要用来写这个**。

函数式编程的语言很多，能用做前端的似乎也不少，包括fsharp, scala等。由于本人主要混在.net平台，所以对fsharp了解比较多。用fsharp写前端主要是靠[Fable](http://fable.io)把fsharp编译成js（**FSharp |> Fable |> Bable |> Webpack |> js**)。

Elmish 是一个设计思想，应该是和mvvm平级，理念类似redux，但是时间是应该更早一点。具体可以参看https://elmish.github.io/elmish/。所以debug的时候也可以用redux的chrome插件查看状态等。

Fable只是一个编译器，除了view其他的都是单纯的逻辑，而view里面的东西可以是很多前端的框架技术比如react（上例用的就是）, react native, 也可以是原生的html，或者vue等，但是Fable社区里面最流行的还是react。所以react生态里的所有控件都可以使用，但是为了有fsharp的类型提醒需要写一些类型声明，这个类似于typescript的.d.ts里写的东西；所以社区里也有人写了一些工具把.d.ts的内容直接翻译成fsharp可使用的类型（目前还没有试过）。当然也可以自行绑定原生js，比如需要引入npm的库的时候就要做这种事情。具体参见http://fable.io/docs/interacting.html

# 俄罗斯方块
代码托管在https://github.com/albertwoo/Tetris

项目初始化是用社区里一个模板生成的[https://github.com/SAFE-Stack]，包括的内容很多，如自动化编译，打包，热更新，测试等，代码也有很多如Client, Server, Test等，目前不需要Server，所以目前的主要代码都在src/Client下面。

`TetrisDomain.fs`定义了俄罗斯方块的基本类型以及一些操作比如操作Block，查看是否相撞，或者清除满足条件的行等。Square是指最小的马赛克方块，Block是指下落的物体：
```fsharp
type Square = { Location: int * int; Color: int * int * int * float }
type BlockType = T | L | J | I | O | Z | RZ | X
type Block = { Type: BlockType; Squares: Square list }
type Action = Rotate | Left | Right | Down
...
```
`Tetris.fs`相当于写了一个组件，包括了状态，更新状态，以及界面的一些东西:
```fsharp
type Model = {
    SquareSize: int
    Boundry: int * int
    AllSquares: Square list
    PreviewBlock: Block option
    MovingBlock: Block option
    PrectionBlock: Block option
    IsOver: bool
    Score: int
    DefaultSpeed: int
    Speed: int
    SpeedCount: int }
type Msg = | Action of Action | ReachBottom | ReachLeft | ReachRight
...
```
`App.fs`是整个程序的入口，会把Tetris定义的东西整合进来，也包含了一些界面的布局，开始，暂停，重新开始等操作。
```fsharp
type Model = {
    Tetris: Tetris.Model
    TimeCost: int
    TouchStartPoint: (float * float) option
    TouchMovingPoint: (float * float) option
    TouchTime: DateTime option
    IsPaused: bool
    IsRestarting: bool
    HideDetail: bool }

type Msg =
    | TetrisMsg of Tetris.Msg
    | BeginRestart | CancelRestart | Restart
    | Tick
    | TouchStart of float * float | TouchMove of float * float | TouchEnd of float * float
    | Pause | Continue
    | HideDetail
...
```
整个程序的组件的拆分没有做得很好，逻辑不是很清晰，作为学习勉强接受吧。
整个效果如下，也可以[在线体验](https://albertwoo.github.io/TetrisHtml/%23root)，触控最佳，键盘勉强可用：


# 结语

函数式编程似乎离成为主流还有很长的路，在现代的服务器方面的应用应该比较多，毕竟微服务的流行导致框架和语言的选择变得更灵活。但是在前端方面的应用很少，android, xamarin等本地应用的开发都大量使用mvvm的设计模式。以前我开发过wpf，都是基于mvvm，现在开发angular几乎自然上手，设计思想如出一辙。React倒是特立独行，包括其中流行的redux状态管理也很有独到之处。vue给我的感觉就显得抱负很大什么都想做，但是个人浅尝后最后还是选择了angular，毕竟有typescript的完美融合。

因为项目启动后已经使用了angular，但是也想利用redux的一些先进思想，后来尝试ngrx，但是发现需要写太多冗余的代码，而且分散在很多不同的文件，加之js的语言特性，写actions, reducers, selectors的时候简直婆婆妈妈，让我失去耐心，冗余代码几乎赶上业务逻辑代码，所以最后还是放弃。不过angular本身的设计已经在我的项目够用了，除了有点啰嗦。

面向对象还大行其道，并且对各种问题都有着成熟的解决方案，上面所言都是在下实践所得的经验，没有严格的理论支持，纯属个人妄言，听听就是，也可嗤之以鼻。
