a selfie retweet bot<br>
自拍转推机器人
===
部署
===

1.下载EXE中的全部文件
2.修改default.conf定义文件
*AccessToken：推特的AccessToken
*AccessTokenSecret：AccessTokenSecret
*ConsumerKey：推特的ConsumerKey
*ConsumerSecret：推特的ConsumerSecret
*MyTwitterID：自己的推特ID，在hometimeline中会过滤掉
*DBType：数据库类型
*DBConnectString：数据库连接
*RecognizerKey：微软牛津的apikey，只找女性照片，RecognizerService设置为false时不用
*RecognizerTempPath：本地临时文件夹
*RecognizerService：可以设置为true/false，使用微软牛津的apikey
*Bot：需要定期执行的文件，和间隔时间（分）
3.定义文件修改完后，执行mainbot一次，会生成数据库SelfBot.sqlite
4.打开sqlite编辑器，添加定义
*

===
原理
===
```markdown
1.抓推
2.图片下载
3.人脸识别
4.转推
```

关键字列表：<br>
===
```markdown
*defines/IDBlackList.txt  //ID黑名单，明显非自拍账户或者新闻账户的id
*defines/NameBlockKeywords.txt //ID名称过滤表，如XX新闻等
*defines/SearchKeywords.txt //自拍搜索关键字
*defines/blockKeywords.txt //自拍过滤过关键字，如RT等
```
