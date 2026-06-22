# Changelog

## [1.5.1](https://github.com/sondresjolyst/nstuning-api/compare/v1.5.0...v1.5.1) (2026-06-22)


### Dependencies

* bump `Mapster` and `Mapster.DependencyInjection` from 10.0.7 to 10.0.8 ([#37](https://github.com/sondresjolyst/nstuning-api/issues/37)) ([8a39213](https://github.com/sondresjolyst/nstuning-api/commit/8a392139878f80a640de7cde61d62c358dc8250b))
* bump `Microsoft.AspNetCore.Authentication.JwtBearer` from 10.0.8 to 10.0.9 ([#38](https://github.com/sondresjolyst/nstuning-api/issues/38)) ([eebf1ea](https://github.com/sondresjolyst/nstuning-api/commit/eebf1ea621befd76462474647d8df27d3303c9db))
* bump `Microsoft.AspNetCore.Identity.EntityFrameworkCore` from 10.0.8 to 10.0.9 ([#39](https://github.com/sondresjolyst/nstuning-api/issues/39)) ([6af6c87](https://github.com/sondresjolyst/nstuning-api/commit/6af6c8702aeae0304d86fca5671f225c5ff77cce))
* bump `Microsoft.AspNetCore.OpenApi` from 10.0.8 to 10.0.9 ([#40](https://github.com/sondresjolyst/nstuning-api/issues/40)) ([b097caa](https://github.com/sondresjolyst/nstuning-api/commit/b097caac91dd3f6e49fd7331024c71c05fbf8b6e))
* bump `Microsoft.EntityFrameworkCore` from 10.0.8 to 10.0.9 ([#41](https://github.com/sondresjolyst/nstuning-api/issues/41)) ([78f87bc](https://github.com/sondresjolyst/nstuning-api/commit/78f87bcca32b74eafd16ed23e89350a5c10ed48f))

## [1.5.0](https://github.com/sondresjolyst/nstuning-api/compare/v1.4.0...v1.5.0) (2026-06-17)


### Features

* decouple engine catalog from the vehicle tree ([#34](https://github.com/sondresjolyst/nstuning-api/issues/34)) ([a158120](https://github.com/sondresjolyst/nstuning-api/commit/a158120fbf821c904fdd03c8805957ecb464d96a))

## [1.4.0](https://github.com/sondresjolyst/nstuning-api/compare/v1.3.0...v1.4.0) (2026-06-16)


### Features

* dyno hub/engine figures + dyno date, company settings, webp variants ([#29](https://github.com/sondresjolyst/nstuning-api/issues/29)) ([780ceb9](https://github.com/sondresjolyst/nstuning-api/commit/780ceb9aaa7e4d927b2d6a5a1017280a16926b90))


### Bug Fixes

* harden image pipeline (EXIF, dimension guard, nosniff, cache) ([#31](https://github.com/sondresjolyst/nstuning-api/issues/31)) ([270e477](https://github.com/sondresjolyst/nstuning-api/commit/270e47790483d71f654b8c9f68b1ad6ae69d38bd))
* persist new cover image on update  ([#33](https://github.com/sondresjolyst/nstuning-api/issues/33)) ([535bae3](https://github.com/sondresjolyst/nstuning-api/commit/535bae3fd1dcda015969d5ea69f13c3493c0f0da))


### Performance Improvements

* downsample image decode to bound webp memory (fixes OOMKill) ([#32](https://github.com/sondresjolyst/nstuning-api/issues/32)) ([c213d13](https://github.com/sondresjolyst/nstuning-api/commit/c213d138da81ea1ea0e01d720b99073fb14724fa))

## [1.3.0](https://github.com/sondresjolyst/nstuning-api/compare/v1.2.0...v1.3.0) (2026-06-16)


### Features

* editable company address ([#18](https://github.com/sondresjolyst/nstuning-api/issues/18)) ([867b493](https://github.com/sondresjolyst/nstuning-api/commit/867b493b66ea4026f0ef9a1ff2ad065013f08782))

## [1.2.0](https://github.com/sondresjolyst/nstuning-api/compare/v1.1.0...v1.2.0) (2026-06-13)


### Features

* password reset and change password ([#16](https://github.com/sondresjolyst/nstuning-api/issues/16)) ([ec7d80c](https://github.com/sondresjolyst/nstuning-api/commit/ec7d80cceb2d2a0b3ad8604d584deed4f89edbbc))

## [1.1.0](https://github.com/sondresjolyst/nstuning-api/compare/v1.0.0...v1.1.0) (2026-06-13)


### Features

* Admin platform, vehicle catalog, and vertical slice migration ([#15](https://github.com/sondresjolyst/nstuning-api/issues/15)) ([232fff8](https://github.com/sondresjolyst/nstuning-api/commit/232fff881704f537c4c0d257c519472551684f99))
* branding ([#3](https://github.com/sondresjolyst/nstuning-api/issues/3)) ([20a14aa](https://github.com/sondresjolyst/nstuning-api/commit/20a14aa0ab1d12c87296892fdf37c643f8cce3ac))

## 1.0.0 (2026-06-11)


### Features

* initial nstuning-api ([0bae685](https://github.com/sondresjolyst/nstuning-api/commit/0bae685bf100f7d2bf3579f24f34a242920bef8d))

## Changelog
