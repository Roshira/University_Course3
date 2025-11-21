package lab;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.*;

public class AppTest {

    @Test
    @DisplayName("Main method should run without exceptions")
    void testMainMethod() {
        assertDoesNotThrow(() -> {
            App.main(new String[]{});
        }, "App.main() threw an unexpected exception");
    }
}
