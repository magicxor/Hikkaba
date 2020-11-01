Hikkaba [pre-alpha]
![Azure deploy](https://github.com/magicxor/Hikkaba/workflows/Azure%20deploy/badge.svg)
=====

Hikkaba is an imageboard written in ASP.NET Core and Entity Framework with minimal JavaScript usage.


Features
========

- [x] Multiple files per post
   * Audio
   * Video
   * Pictures
   * Documents
- [x] Thumbnail generation ([ImageSharp](https://github.com/JimBobSquarePants/ImageSharp))
- [x] BBCode markup support ([Codekicker.BBCode](https://archive.codeplex.com/?p=bbcode))
   * b, i, u, s, pre, sub, sup, spoiler, quote BBCodes are availiable
   * `>>postId` - a link to the post in the current thread
- [x] SAGE support
- [x] URI detection
   * http://, https://, ftp:// links autodetection
- [x] Captcha ([DNTCaptcha.Core](https://github.com/VahidN/DNTCaptcha.Core))
- [x] Server-side paging ([Sakura.AspNetCore.PagedList](https://github.com/sgjsakura/AspNetCore))
- [x] Thread-local user hashes (can be enabled for each thread separately)
- [x] Search
- [x] Display a datetime in the current user timezone ([Moment.js](http://momentjs.com/))
- [x] Support for multiple file storage engines ([20|20 Storage](https://github.com/2020IP/TwentyTwenty.Storage))
   * Local File System Storage (enabled by default)
   * Azure Blob Storage
   * Amazon S3
   * Google Cloud Storage
- [x] Administration panel [in progress]
- [x] Сategory specific moderators [in progress]
- [x] Moderation functions [in progress]
- [x] Ban system - by IP or IP range [in progress]
- [ ] Custom file size limit
- [ ] Custom post size limit
- [x] Custom attachment count limit
- [ ] Identity lockout
- [ ] Custom maximum number of threads per category
- [ ] Archive old threads
- [ ] Media gallery
- [ ] API
- [ ] Embedding of youtube, vimeo, coub, twitter, instagram objects
- [ ] Detection of attachment duplicates per thread
- [ ] Image compression
- [ ] Custom primary key type (guid/long/int/etc) [in progress]
- [x] Docker


Screenshots
========

## Home page
![Home page](http://i.imgur.com/VSqxCqE.png)

---

## Reply form
![Reply form](http://i.imgur.com/aVO3paD.png)

---

## Thread
![Thread](http://i.imgur.com/OLJ8YS6.png)

---

## Search
![Search](http://i.imgur.com/wkp4WoR.png)


EF Migrations
========
```powershell
$env:Hikkaba_ConnectionStrings__DefaultConnection="Server=(localdb)\mssqllocaldb;Database=Hikkaba;Integrated Security=true;"; dotnet ef migrations list --project Hikkaba.Data --startup-project Hikkaba.Web --verbose
```
