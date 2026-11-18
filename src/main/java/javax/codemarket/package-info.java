/**
 * Stubs of the Java scripting API in the game
 * <a href="https://store.steampowered.com/app/5451260/">Code Market</a>, for IDE completion and type
 * checking.
 *
 * <p>This is an <b>unofficial</b> library, made with the developer's permission but not by them.
 *
 * <p>Start from {@link javax.codemarket.GameScript}: extend it, and its {@code robot} and {@code store}
 * fields are in scope. The types match what the game exposes to Java scripts: {@link javax.codemarket.Robot},
 * {@link javax.codemarket.Store}, {@link javax.codemarket.Shelf}, {@link javax.codemarket.Cell},
 * {@link javax.codemarket.Customer}, {@link javax.codemarket.Order}, and the enums
 * {@link javax.codemarket.Direction}, {@link javax.codemarket.Product}, {@link javax.codemarket.Entity}
 * and {@link javax.codemarket.Season}.
 *
 * <p>The package is under {@code javax} so that its import line is harmless in the game, which skips
 * {@code java.*} and {@code javax.*} imports.
 *
 * <p>The game runs Java on its own interpreter, which supports only part of the JDK. Code that compiles
 * here can still fail in the game if it uses a JDK class the game lacks.
 */
package javax.codemarket;
