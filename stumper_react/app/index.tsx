import { useFonts } from "expo-font";
import * as SplashScreen from "expo-splash-screen";
import { useEffect, useState } from "react";
import { Text, View, StyleSheet, TextInput } from "react-native";
import Flipboard from "./components/flipboard";
import Graph, { RawNode } from "./game/graph";
import rawGraph from "./data/raw_graph.json";

SplashScreen.preventAutoHideAsync();
const castRawGraph = rawGraph as RawNode[];
const graph = new Graph(castRawGraph);

export default function Index() {
  // Load fonts
  const [loaded, error] = useFonts({
    ChivoMono: require("../assets/fonts/ChivoMono-VariableFont_wght.ttf"),
    Chivo: require("../assets/fonts/Chivo-VariableFont_wght.ttf"),
  });

  useEffect(() => {
    if (loaded || error) {
      SplashScreen.hideAsync();
    }
  }, [loaded, error]);

  if (!loaded && !error) {
    return null;
  }

  // State
  const [currentNode, changeCurrentNode] = useState(
    graph.getRandomStartingNode(),
  );
  const [visited, changeVisited] = useState(
    new Set<string>([currentNode.word]),
  );
  const [currentWord, changeCurrentWord] = useState(currentNode.word);
  const [previousWord, changePreviousWord] = useState("");

  function HandleMove(word: string) {
    word = word.toUpperCase();
    const maybeNext = graph.query(word);

    if (visited.has(word) || maybeNext == null) {
      // TODO strikes etc.
      return;
    }

    changeVisited(new Set<string>([...visited, word]));

    changeCurrentNode(maybeNext);
    changePreviousWord(currentWord);
    changeCurrentWord(word);
  }

  return (
    <View>
      <TextInput
        editable
        onSubmitEditing={(e) => {
          HandleMove(e.nativeEvent.text);
        }}
      />
      <Flipboard currentWord={currentWord} previousWord={previousWord} />
    </View>
  );
}
