package javax.codemarket;

/**
 * Base class for a Code Market program. Extend it and write your program in {@link #main()}:
 *
 * <pre>
 * import javax.codemarket.*;
 *
 * public class Main extends GameScript {
 *     &#64;Override
 *     protected void main() {
 *         robot.move(Direction.UP);
 *     }
 * }
 * </pre>
 *
 * <p>To run it in the game, paste the file in and delete {@code extends GameScript}: the game has no
 * such class, but it does run {@code main()} and provides {@code robot} and {@code store} itself.
 * The import can stay, since the game skips {@code java.*} and {@code javax.*} imports.
 *
 * <p>Outside the game, {@link #robot} and {@link #store} are stand-ins: every call on them throws
 * {@link UnsupportedOperationException}. They exist so your IDE can check and complete your code.
 */
public abstract class GameScript {
    /** The robot running this program. */
    protected final Robot robot = new StubRobot();

    /** The store the robot works in. */
    protected final Store store = new StubStore();

    /** Your program. The game runs it once, top to bottom, each time you press "Run". */
    protected abstract void main();

    static UnsupportedOperationException notInGame(String call) {
        return new UnsupportedOperationException(call + " only works inside Code Market: this library is a stub"
                + " for IDE completion. Paste your code into the game to run it.");
    }
}
