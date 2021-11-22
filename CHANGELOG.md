# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added

- Loading of additional settings file: `/settings/appsettings.json` if it exists
- Use of forwarded headers if "UseForwardedHeaders" environment variable is set

### Changed

- Comment out Seq sink pointing at localhost in `appsettings.json`

## [1.0.0] - 2021-11-17

### Added

- Boilerplate ASP.NET 6 Web project
- Info page showing info and environment information
- Distributed caching through Redis or the built-in in-memory cache
- Environment variable "RedisConfiguration" to configure Redis
- Environment variable "RedisInstance" to differentiate multiple copies of the application using the same caching server
- Environment variable "Instance" to differentiate between different copies of the application sharing a backend
- Serilog logging
- Serilong Seq sink
- Dockerfile
- build.bash build script
- GitHub action to build the project using Docker
- GitHub action to look for outdated dependencies

[unreleased]: https://github.com/olivierlacan/keep-a-changelog/compare/a5ca13d590d183b6a748d8e380545b73fd1f92a8...head
[1.0.0]: https://github.com/k7hpn/k8stest/commit/a5ca13d590d183b6a748d8e380545b73fd1f92a8
