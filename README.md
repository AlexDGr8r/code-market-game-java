# Code Market Java API (unofficial)

Write your robot programs for the Steam game **[Code Market](https://store.steampowered.com/app/5451260/)**
in a real Java IDE, like IntelliJ IDEA or VS Code, instead of the in-game editor. You get:

- **Autocomplete:** type `robot.` and see everything your robot can do.
- **Error checking as you type:** typos and wrong types are underlined before you paste anything into the game.
- **The game's own documentation** when you hover over a method.

> [!IMPORTANT]
> This is an **unofficial, fan-made library**. It is published with the permission of Code Market's
> developer, but it is not made or supported by them. Please don't send them bug reports about it:
> [open an issue here](https://github.com/alexdgr8r/code-market-game-java/issues) instead.
> See [NOTICE.md](NOTICE.md).

```java
import javax.codemarket.*;

public class Main extends GameScript {
    @Override
    protected void main() {
        Order order = robot.takeOrder();
        if (order != null) {
            for (Product p : order.getProducts()) {
                System.out.println(p.getName() + " x" + order.getCount(p));
            }
        }
    }
}
```

## Contents

- [Quick start: the starter project](#quick-start-the-starter-project) (recommended for beginners)
- [Running your code in the game](#running-your-code-in-the-game)
- [Adding the library to an existing project](#adding-the-library-to-an-existing-project)
- [What's in the API](#whats-in-the-api)
- [Troubleshooting](#troubleshooting)
- [For contributors: building, checking and updating](#for-contributors)
- [License](#license)

## Quick start: the starter project

New to Java tooling? Start here. The starter project is a folder that's already set up: open it, write
code, done.

**1. Install a JDK and an IDE.**

- A **JDK** (Java Development Kit) is what lets your computer compile Java. Install
  [Eclipse Temurin](https://adoptium.net/), the latest LTS version. Any JDK 11 or newer works.
- An **IDE** is the editor you'll write code in. [IntelliJ IDEA Community Edition](https://www.jetbrains.com/idea/download/)
  is free and the easiest to start with. [VS Code](https://code.visualstudio.com/) with the
  *Extension Pack for Java* also works.

You don't need to install Maven (the build tool) yourself. The project comes with small `mvnw`
scripts that fetch it for you.

**2. Download the starter project.**

Go to the [latest release](https://github.com/alexdgr8r/code-market-game-java/releases/latest) and
download `code-market-starter-<version>.zip` (or the `.tar.gz` on macOS/Linux). Unzip it anywhere.

**3. Open it in your IDE.**

- **IntelliJ IDEA:** *File → Open…*, choose the unzipped `code-market-starter` folder, then *Open as
  Project*. Wait for the progress bar at the bottom to finish.
- **VS Code:** *File → Open Folder…*, choose the folder, and accept when it offers to import the Maven project.

**4. Write code.**

Open `src/main/java/Main.java`. It's a full working example that serves one customer: it walks to the
register, takes the order, picks each product off the shelves and packs it into the bag. Read it, change
it, or replace it with your own program.

**5. Paste it into the game.** See the next section.

## Running your code in the game

1. Copy your whole file.
2. Paste it into Code Market's code editor.
3. **Delete `extends GameScript`** from the class line, so `public class Main extends GameScript {`
   becomes `public class Main {`. The game doesn't have a `GameScript` class and will refuse the script
   with "the parent class GameScript was not found". Everything else can stay.
4. Run it in the game.

**You can't run the program from your IDE.** Outside the game, `robot` and `store` are stand-ins and
every call on them throws `UnsupportedOperationException`. The library only exists to help you *write*
the code. (The `Product` enum works anywhere, apart from `isInSeason()`.)

Things to know, checked against the game's own compiler:

- **The game runs your `main()` method.** `@Override` and `protected` are fine. The game provides `robot`
  and `store`.
- **Keep the `import javax.codemarket.*;` line.** The game ignores `java.*` and `javax.*` imports, which
  is why this library lives in the `javax` package.
- **Other imports mean your other in-game files.** In the game, `import foo.Bar;` refers to a file in
  your in-game project, not to a library.
- **Helper methods that use `robot` or `store` can't be `static`.** They're fields of `GameScript`.
- **The game only has part of Java.** `ArrayList`, `HashMap`, `String`, `Math` and similar classes are
  there; some others (for example `ConcurrentHashMap`) are not. Your IDE can't tell the difference. If
  you're on Windows, [the checker](#check-a-script-with-the-games-compiler) can.
- **Grid coordinates:** `Direction.UP` is y + 1, and `Direction.RIGHT` is x + 1.

## Adding the library to an existing project

If you already have a Maven or Gradle project, add the library as a dependency. It's only needed at
compile time (the game has its own copy), so use `provided` / `compileOnly`.

> [!NOTE]
> The library isn't on Maven Central yet. Until it is, build it from source and install it locally:
> clone this repo and run `./mvnw install` (see [Building](#building)). Then add `mavenLocal()` to
> your Gradle repositories.

**Maven** (`pom.xml`):

```xml
<dependency>
    <groupId>io.github.alexdgr8r.codemarket</groupId>
    <artifactId>code-market-game-api</artifactId>
    <version>0.1.0</version>
    <scope>provided</scope>
</dependency>
```

**Gradle** (`build.gradle.kts`):

```kotlin
dependencies {
    compileOnly("io.github.alexdgr8r.codemarket:code-market-game-api:0.1.0")
}
```

**No build tool?** Each [release](https://github.com/alexdgr8r/code-market-game-java/releases) also has
the plain JARs. In IntelliJ: *File → Project Structure → Libraries → + → Java*, pick
`code-market-game-api-<version>.jar`, and attach the `-sources.jar` so the docs show up. In VS Code, add
the JAR under *Java Projects → Referenced Libraries*.

## What's in the API

| Type | How you get it | What it is |
|---|---|---|
| `GameScript` | `extends GameScript` | Base class: gives you the `robot` and `store` fields and a `main()` to write |
| `Robot` | `robot` | The robot running the program: moving, picking, shelving, taking orders, bagging |
| `Store` | `store` | Store layout, registers, stock, clock, money, deliveries |
| `Shelf` | `robot.getShelf()` | The rack in front of the robot |
| `Cell` | `store.find(...)` | A tile of the store grid |
| `Customer` | `robot.getCustomer()` | A customer waiting at a register |
| `Order` | `robot.takeOrder()` | What the customer wants, and what's left to pack |
| `Direction`, `Product`, `Entity`, `Season` | | Enums. `Product` carries each product's base price, rack, season and flags. |

Everything lives in the `javax.codemarket` package. The Javadoc is the game's own in-game reference
text, so it includes the same warnings the game gives, like fines for selling alcohol after 22:00 or
to children.

**Where the API comes from:** the game is built with Unity and runs Java scripts on its own interpreter.
It describes its scripting API as data tables inside its DLLs, with each method's Java signature and its
reference text. `tools/extractor` reads those tables from the installed game into
[`api/code-market-api.json`](api/code-market-api.json), and `tools/generate.py` turns that into the
Java sources. So the signatures and docs are the game's own, not copied by hand. The current files
come from Steam build 26723545.

## Troubleshooting

**`'mvnw' is not recognized` / `./mvnw: Permission denied`**
On Windows use `mvnw.cmd` (or `.\mvnw.cmd` in PowerShell). On macOS/Linux run `chmod +x mvnw` once.

**`JAVA_HOME is not set` or `java: command not found`**
Install a JDK (see [Quick start](#quick-start-the-starter-project)) and restart your terminal. If it
still fails, set the `JAVA_HOME` environment variable to the JDK's install folder.

**`Could not find artifact io.github...code-market-game-api`**
The library isn't on Maven Central yet. Run `./mvnw install` in a clone of this repo first.

**`UnsupportedOperationException: ... only works inside Code Market`**
You ran the program outside the game. That's expected: paste it into the game to run it.

**The game says "the parent class GameScript was not found"**
Delete `extends GameScript` from your class line before running it in the game.

**The game says a class doesn't exist, but it compiles in my IDE**
The game only supports part of the Java standard library. Use a more common class instead
(e.g. `HashMap` instead of `ConcurrentHashMap`).

## For contributors

### Building

You need JDK 11 or newer. The output targets Java 8.

```bash
./mvnw install
```

(On Windows: `mvnw.cmd install`.) This writes to `target/`:

- `code-market-game-api-<version>.jar`, plus `-sources.jar` and `-javadoc.jar`
- `code-market-starter-<version>.zip` and `.tar.gz`: the starter project from `examples/`, for GitHub releases

`install` also copies the JARs into your local Maven repository (`~/.m2`), so the starter project and
your own projects can use them. To check the starter project compiles against what you just built:

```bash
cd examples
./mvnw package
```

### Check a script with the game's compiler

*Windows only, needs the game installed.* The game's compiler can run outside the game, so you can catch
game-only errors (like a missing JDK class) before pasting. The checker deletes `extends GameScript`
first, the same way you would:

```powershell
.\tools\check.ps1 examples\src\main\java\Main.java
```

```text
examples/src/main/java/Main.java: OK
bad.java:5:15: error: robot has no method mvoe(): did you mean move()?
```

If the game isn't at the default Steam path, pass `-GameDir` or set `CODE_MARKET_DIR`. It also needs a
.NET 8 or newer runtime (`dotnet`). It builds itself with the .NET Framework C# compiler that ships with
Windows, so you don't need a .NET SDK.

Limits: it only compiles your code, it doesn't run it. And it checks one file at a time, so imports
between your in-game files aren't resolved.

### After a game update

```powershell
.\tools\update-api.ps1
.\mvnw.cmd install
```

This re-extracts `api/code-market-api.json` from the installed game and regenerates the sources. It
needs Python 3 and the same .NET runtime as the checker. The generator stops with an error if the game
adds or removes script types or `Product` methods, so changes can't slip through unnoticed. Review the
result with `git diff`.

### Using it with an AI assistant

Give the assistant the in-game rules in [Running your code in the game](#running-your-code-in-the-game)
and the sources JAR (or `api/code-market-api.json`), and ask it to run `tools\check.ps1` on whatever
it writes.

### Releasing

1. Set the new version in `pom.xml` (`<version>`) and `examples/pom.xml` (`<code-market-game-api.version>`),
   and add an entry to [CHANGELOG.md](CHANGELOG.md).
2. Commit, then push a matching tag, e.g. `git tag v0.2.0 && git push origin v0.2.0`.

The [Release workflow](.github/workflows/release.yml) checks the tag matches both POMs, builds
everything and creates a GitHub release with the JARs and the starter project attached.

### Repository layout

```text
api/code-market-api.json       API extracted from the game (source of truth for the generator)
src/main/java/javax/codemarket/
  GameScript.java              base class with robot and store (hand-written)
  package-info.java            package docs (hand-written)
  *.java                       everything else is generated: don't edit by hand
src/assembly/starter.xml       how the starter project zip is packed
examples/                      the starter project (a standalone Maven project)
  src/main/java/Main.java      example that serves one customer end to end
tools/extractor/Extract.cs     reads the API tables out of the game's DLLs
tools/checker/Check.cs         compiles a script with the game's compiler
tools/generate.py              JSON -> Java sources
tools/*.ps1                    Windows scripts that run the tools above
```

## License

The code in this repository is under the [MIT License](LICENSE). The API names, signatures and
reference text come from Code Market and belong to its developer; they're included with permission.
See [NOTICE.md](NOTICE.md) for details. This project contains no game code or assets.
