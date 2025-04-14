# Hikkaba [Pre-alpha]

Hikkaba is an imageboard written using ASP.NET Core and Entity Framework Core, designed with minimal JavaScript usage.

## Features

### Core Functionality
- [x] **Multiple files per post:** Supports attaching various file types:
   * Audio
   * Video
   * Pictures
   * Documents
- [x] **SAGE support:** Prevents thread bumping on reply
- [x] **Search:** Allows full-text searching through posts
- [x] **Server-side paging:** Efficiently handles navigation through category indexes ([Sakura.AspNetCore.PagedList](https://github.com/sgjsakura/AspNetCore))
- [x] **Thread-local user hashes:** Option to display unique user identifiers ([Blake3.NET](https://github.com/xoofx/Blake3.NET))
- [x] **Docker support:** Ready for containerized deployment
- [ ] Archive old threads
- [ ] API for third-party integration
- [ ] Internationalization (multi-language support)
- [ ] User-created boards
- [ ] Virtual `/all` category (shows threads from all non-hidden categories)
- [ ] Tor and I2P network support

### Content & Formatting
- [x] **Thumbnail generation:** Creates thumbnails for image and video files ([ImageSharp](https://github.com/SixLabors/ImageSharp))
- [x] **BBCode markup:** Supports common formatting tags ([BBCodeParser](https://github.com/quilin/BBCodeParser)):
   * `[b]`, `[i]`, `[u]`, `[s]`, `[code]`, `[sub]`, `[sup]`, `[spoiler]`, `[quote]`
   * `>>postId` - Creates a link to another post within the same thread
- [x] **URI detection:** Automatically converts `http://`, `https://`, and `ftp://` links
- [x] **Timezone display:** Shows post datetimes adjusted to the current user's timezone ([Moment.js](http://momentjs.com/))
- [x] Post backlink display (shows replies to a post)
- [ ] Media gallery view
- [ ] Embedding content from YouTube, Vimeo, Coub, Twitter, Instagram, etc.
- [ ] Detection of duplicate attachments within a thread
- [ ] Tripcodes
- [ ] Visual style/theme switching
- [ ] Post reactions (e.g., likes, dislikes, etc.)
- [ ] Display country flags for posters (using GeoIP)
- [ ] Display user agent icons (e.g. browser type or OS type)
- [ ] Post highlighting via user hash click
- [ ] Truncate long messages by line count
- [ ] Fetch new replies automatically

### Storage
- [x] **Multiple storage backends:** Flexible file storage options ([20|20 Storage](https://github.com/2020IP/TwentyTwenty.Storage)):
   * Local File System (default)
   * Azure Blob Storage
   * Amazon S3
   * Google Cloud Storage

### Moderation & Administration
- [x] **Captcha:** Protects against automated posting ([DNTCaptcha.Core](https://github.com/VahidN/DNTCaptcha.Core))
- [x] Administration panel (In Progress)
- [x] Per-category moderator assignments (In Progress)
- [x] Moderation functions (In Progress)
- [x] **Ban system:** Supports banning by IP address or range (In Progress)
  * Board-wide or category-specific bans
  * Options for post deletion upon banning: single post, all posts in the category, or all posts board-wide
- [x] **Thread locking:** Prevents further replies
- [x] **Thread stickying:** Pins threads to the top
- [ ] Identity lockout (Account security feature)
- [ ] Two-factor authentication (TOTP)
- [ ] Passcodes (user captcha bypass)
- [ ] Delete posts via password
- [ ] Report posts
- [ ] DNSBL integration (spam prevention)
- [ ] Move threads between boards
- [ ] Modlog (audit trail for staff actions)
- [ ] Wordfilter (regex support, configurable actions: replace, hide, deny)

### Limits & Configuration
- [x] Custom attachment count limit per post
- [x] Maximum size per individual file attachment
- [x] Maximum total size of all attachments per post
- [x] Custom maximum number of threads per category
- [x] Allowed file extensions filter
- [x] Bump limit (per category and/or per thread)
- [x] Cycling thread mode (deletes old posts when limit is reached)

### Observability
- [x] Dashboards ([Grafana](https://grafana.com/))
- [x] Metrics ([OpenTelemetry](https://opentelemetry.io/docs/languages/dotnet/getting-started/#send-data-to-a-collector), [Prometheus](https://prometheus.io/))
- [x] Tracing ([OpenTelemetry](https://opentelemetry.io/docs/languages/dotnet/getting-started/#send-data-to-a-collector), [Grafana Tempo](https://grafana.com/oss/tempo/))
- [x] Structured Logging ([OpenTelemetry](https://opentelemetry.io/docs/languages/dotnet/getting-started/#send-data-to-a-collector), [Grafana Loki](https://grafana.com/oss/loki/), [NLog](https://nlog-project.org/))
- [x] Health Checks

### Testing
- [x] Unit tests ([NUnit](https://nunit.org/))
- [x] Integration tests ([NUnit](https://nunit.org/), [Testcontainers](https://dotnet.testcontainers.org/))

## Screenshots

### Home page
![Home page](http://i.imgur.com/VSqxCqE.png)

---

### Reply form
![Reply form](http://i.imgur.com/aVO3paD.png)

---

### Thread
![Thread](http://i.imgur.com/OLJ8YS6.png)

---

### Search
![Search](http://i.imgur.com/wkp4WoR.png)
