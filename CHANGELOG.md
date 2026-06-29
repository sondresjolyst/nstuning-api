# Changelog

## [1.6.1](https://github.com/sondresjolyst/nstuning-api/compare/v1.6.0...v1.6.1) (2026-06-29)


### Dependencies

* bump `actions/checkout` from 6.0.3 to 7.0.0 ([#45](https://github.com/sondresjolyst/nstuning-api/issues/45)) ([8f4cf47](https://github.com/sondresjolyst/nstuning-api/commit/8f4cf47ab668551370605059f43e9eca9e1856b0))
* bump `equinor/ops-actions/.github/workflows/docker.yml` from 9.38.2 to 9.38.3 ([#47](https://github.com/sondresjolyst/nstuning-api/issues/47)) ([3350b9e](https://github.com/sondresjolyst/nstuning-api/commit/3350b9e43aff4bac0e28d62436740402045a3d57))
* bump `equinor/ops-actions/.github/workflows/release-please-manifest.yml` from 9.38.2 to 9.38.3 ([#46](https://github.com/sondresjolyst/nstuning-api/issues/46)) ([fa4fba5](https://github.com/sondresjolyst/nstuning-api/commit/fa4fba5d1f29bb07a97965949d3fa45a988170d9))
* Bump `Microsoft.EntityFrameworkCore.Design` and `Microsoft.EntityFrameworkCore.Tools` from 10.0.8 to 10.0.9 ([#50](https://github.com/sondresjolyst/nstuning-api/issues/50)) ([8c1ba42](https://github.com/sondresjolyst/nstuning-api/commit/8c1ba4293fbf84d59bc0b9212e51f230bd1a4564))
* bump `Microsoft.EntityFrameworkCore.InMemory` from 10.0.8 to 10.0.9 ([#49](https://github.com/sondresjolyst/nstuning-api/issues/49)) ([7424e5a](https://github.com/sondresjolyst/nstuning-api/commit/7424e5a7bc560ae23f7e36281dce2cfd29979c9f))
* bump `Microsoft.Extensions.Logging.Abstractions` from 10.0.8 to 10.0.9 ([#51](https://github.com/sondresjolyst/nstuning-api/issues/51)) ([3afa4d2](https://github.com/sondresjolyst/nstuning-api/commit/3afa4d29f408fde6a2790081e6a78c5f8ed39845))
* bump `SkiaSharp.NativeAssets.Linux.NoDependencies` from 2.88.9 to 3.119.4 ([#53](https://github.com/sondresjolyst/nstuning-api/issues/53)) ([dbc66ea](https://github.com/sondresjolyst/nstuning-api/commit/dbc66ea018222e3eab5074a115b8347b65718864))
* bump `SkiaSharp` from 2.88.9 to 3.119.4 ([#52](https://github.com/sondresjolyst/nstuning-api/issues/52)) ([2f54e52](https://github.com/sondresjolyst/nstuning-api/commit/2f54e5250a2d0ca077ecb16c52469015e3b3ea51))

## [1.6.0](https://github.com/sondresjolyst/nstuning-api/compare/v1.5.1...v1.6.0) (2026-06-22)


### Features

* **dyno-runs:** displacement & absolute-pressure fields ([#43](https://github.com/sondresjolyst/nstuning-api/issues/43)) ([428f2dd](https://github.com/sondresjolyst/nstuning-api/commit/428f2ddef43c568b288569ef51209c17e0552773))

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
