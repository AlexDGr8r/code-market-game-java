# Code Market starter project

A ready-to-open Java project for writing robot programs for
[Code Market](https://store.steampowered.com/app/5451260/) in a real IDE, with autocomplete, error
checking and the game's own documentation on hover.

> This uses an **unofficial** library made by fans, with the developer's permission. It is not made or
> supported by the developer of Code Market.

## 1. What you need

- **A JDK** (Java Development Kit), version 11 or newer. If you don't have one, install
  [Eclipse Temurin](https://adoptium.net/) (pick the latest LTS version).
- **An IDE.** [IntelliJ IDEA Community Edition](https://www.jetbrains.com/idea/download/) (free) is the
  easiest. [VS Code](https://code.visualstudio.com/) with the *Extension Pack for Java* works too.

You do **not** need to install Maven. The `mvnw` / `mvnw.cmd` scripts in this folder download it for you.

## 2. Open the project

- **IntelliJ IDEA:** *File → Open…*, pick this folder (the one with `pom.xml` in it), and choose
  *Open as Project*. Wait for the progress bar at the bottom to finish.
- **VS Code:** *File → Open Folder…* and pick this folder. Accept when it asks to import the Maven project.

The first time, your IDE downloads the Code Market library. That needs an internet connection.

## 3. Write your program

Open `src/main/java/Main.java`. It's a complete example that serves one customer: it walks to the
register, takes the order, fetches each product from its shelf and packs it into the bag. Change it,
or delete it and start fresh. Type `robot.` or `store.` and your IDE will list everything you can do.

Every program looks like this:

```java
import javax.codemarket.*;

public class Main extends GameScript {
    @Override
    protected void main() {
        robot.move(Direction.UP);
    }
}
```

## 4. Check it compiles

Your IDE underlines mistakes as you type. To check from a terminal, run this in this folder:

```bash
./mvnw package
```

On Windows (Command Prompt or PowerShell) use `mvnw.cmd package` instead. You should see `BUILD SUCCESS`.

## 5. Run it in the game

1. Copy all of `Main.java`.
2. Paste it into the code editor in Code Market.
3. **Delete the words `extends GameScript`** from the `public class Main` line. The game doesn't know
   that class and will refuse to run otherwise. Leave everything else as it is.
4. Run it in the game.

Running `Main` from your IDE won't work: outside the game, every `robot` and `store` call throws an
`UnsupportedOperationException`. The library is only there to help you write the code.

## Tips

- **Keep helper methods non-`static`** if they use `robot` or `store`.
- **The game only has part of Java.** Common classes like `ArrayList`, `HashMap`, `String` and `Math`
  work. Some less common ones don't, and your IDE can't tell you which. If the game reports a missing
  class, use a simpler alternative.
- **Directions:** `Direction.UP` is y + 1 and `Direction.RIGHT` is x + 1.

More help, and how to check your script with the game's own compiler before pasting:
<https://github.com/alexdgr8r/code-market-game-java>
