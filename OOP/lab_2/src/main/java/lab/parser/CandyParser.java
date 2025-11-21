package lab.parser;

import java.util.List;
import lab.model.Candy;

public interface CandyParser {
    List<Candy> parse(String xmlFilePath) throws Exception;
}