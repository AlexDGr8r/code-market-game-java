# Changelog

All notable changes to this project are listed here. Versions follow [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.1.0]

First public release.

- Stubs for the full Code Market Java scripting API (`Robot`, `Store`, `Shelf`, `Cell`, `Customer`,
  `Order`, `Direction`, `Product`, `Entity`, `Season`), generated from Steam build 26723545, with the
  game's own reference text as Javadoc.
- `GameScript` base class that puts `robot` and `store` in scope.
- Starter Maven project, attached to GitHub releases.
- Windows tools to check scripts with the game's own compiler and to regenerate the API after a game update.

[Unreleased]: https://github.com/alexdgr8r/code-market-game-java/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/alexdgr8r/code-market-game-java/releases/tag/v0.1.0
