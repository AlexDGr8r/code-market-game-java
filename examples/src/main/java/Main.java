import javax.codemarket.*;

import java.util.ArrayList;
import java.util.List;

/**
 * Serves one customer: takes their order at a register, fetches each product from its rack, and packs
 * it into the bag. To run it in the game, paste it in and delete {@code extends GameScript}.
 */
public class Main extends GameScript {
    @Override
    protected void main() {
        Cell register = store.find(Entity.REGISTER);
        if (register == null) {
            System.out.println("There's no register in the store");
            return;
        }
        int rx = register.getX();
        int ry = register.getY();
        Direction side = store.getRegisterSide(rx, ry);

        Cell cashier = store.getCashierCell(rx, ry);
        if (cashier == null || !walkTo(cashier.getX(), cashier.getY())) {
            System.out.println("Can't reach the register");
            return;
        }
        robot.turn(side);
        Order order = robot.takeOrder();
        if (order == null) {
            System.out.println("Nobody is waiting");
            return;
        }

        Customer customer = robot.getCustomer();
        for (Product product : order.getProducts()) {
            if (product.isAdultsOnly() && customer != null && !customer.isAdult()) {
                order.remove(product); // selling 18+ goods to a child costs a fine
                continue;
            }
            while (order.getRemaining(product) > 0) {
                if (!fetch(product, order.getRemaining(product))) {
                    System.out.println("Out of " + product.getName() + ", crossing it off");
                    order.remove(product);
                    break;
                }
                Cell packer = store.getPackerCell(rx, ry);
                walkTo(packer.getX(), packer.getY());
                robot.turn(side);
                while (robot.has(product) && order.getRemaining(product) > 0) {
                    robot.putInBag(product);
                }
            }
        }
        System.out.println(order.isDone() ? "Order packed" : "Order not finished");
    }

    /** Walks to a rack of this product and picks up to {@code wanted} items. False if none could be picked. */
    boolean fetch(Product product, int wanted) {
        for (Cell rack : store.findAll(product.getRack())) {
            for (Direction facing : Direction.values()) {
                // Stand on the tile from which facing this way looks at the rack.
                int x = rack.getX() - dx(facing);
                int y = rack.getY() - dy(facing);
                if (!store.getPickupDirections(x, y, product).contains(facing) || !walkTo(x, y)) {
                    continue;
                }
                robot.turn(facing);
                int picked = 0;
                while (picked < wanted && robot.getLoad() < robot.getCapacity() && robot.pick(product)) {
                    picked++;
                }
                if (picked > 0) {
                    return true;
                }
            }
        }
        return false;
    }

    /** Walks to tile (x, y) along a shortest free path, re-planning if a customer steps in the way. */
    boolean walkTo(int x, int y) {
        for (int attempt = 0; attempt < 5; attempt++) {
            List<Direction> path = findPath(robot.getX(), robot.getY(), x, y);
            if (path == null) {
                return false;
            }
            boolean blocked = false;
            for (Direction step : path) {
                if (!robot.move(step)) {
                    blocked = true;
                    break;
                }
            }
            if (!blocked) {
                return true;
            }
        }
        return robot.getX() == x && robot.getY() == y;
    }

    /** Breadth-first search over free tiles. Null if (tx, ty) can't be reached. */
    List<Direction> findPath(int sx, int sy, int tx, int ty) {
        int w = store.getWidth();
        int h = store.getHeight();
        int[][] from = new int[w][h]; // index of the direction used to reach a tile, +1; 0 = not visited
        List<Integer> queue = new ArrayList<>();
        from[sx][sy] = -1;
        queue.add(sx * h + sy);
        Direction[] dirs = Direction.values();
        for (int i = 0; i < queue.size(); i++) {
            int cx = queue.get(i) / h;
            int cy = queue.get(i) % h;
            if (cx == tx && cy == ty) {
                break;
            }
            for (int d = 0; d < dirs.length; d++) {
                int nx = cx + dx(dirs[d]);
                int ny = cy + dy(dirs[d]);
                if (nx < 0 || ny < 0 || nx >= w || ny >= h || from[nx][ny] != 0 || !store.isFree(nx, ny)) {
                    continue;
                }
                from[nx][ny] = d + 1;
                queue.add(nx * h + ny);
            }
        }
        if (from[tx][ty] == 0) {
            return null;
        }
        List<Direction> path = new ArrayList<>();
        int cx = tx;
        int cy = ty;
        while (from[cx][cy] > 0) {
            Direction d = dirs[from[cx][cy] - 1];
            path.add(0, d);
            cx -= dx(d);
            cy -= dy(d);
        }
        return path;
    }

    static int dx(Direction d) {
        return d == Direction.RIGHT ? 1 : d == Direction.LEFT ? -1 : 0;
    }

    static int dy(Direction d) {
        return d == Direction.UP ? 1 : d == Direction.DOWN ? -1 : 0;
    }
}
