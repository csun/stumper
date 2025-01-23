import { useFonts } from "expo-font";
import * as SplashScreen from "expo-splash-screen";
import { useEffect, useRef, useState } from "react";
import { Text, View, StyleSheet, TextInput } from "react-native";
import Flipboard from "./components/flipboard";
import Timer from "./components/timer";
import Graph, { RawNode } from "./game/graph";
import rawGraph from "./data/raw_graph.json";

SplashScreen.preventAutoHideAsync();
const castRawGraph = rawGraph as RawNode[];
const graph = new Graph(castRawGraph);

const TIME_INITIAL = 120;
const TIME_MAX = 120;
const TIME_PENALTY = -15;
const TIME_GAIN = 15;

export default function Index() {
  // State
  const [currentNode, changeCurrentNode] = useState(
    graph.getRandomStartingNode(),
  );
  const [currentWord, changeCurrentWord] = useState(currentNode.word);
  const [previousWord, changePreviousWord] = useState("");
  const [currentPlayer, changeCurrentPlayer] = useState(0);
  const [remainingTimes, changeRemainingTimes] = useState([120, 120]);

  // Refs
  const lastTickRef = useRef(Date.now());
  const visitedRef = useRef(new Set<string>(currentWord));

  // State Updates
  function IncrementPlayerTime(player: number, amount: number) {
    changeRemainingTimes(
      remainingTimes.map((t, i) => {
        if (i == player) {
          return Math.max(0, Math.min(t + amount, TIME_MAX));
        } else {
          return t;
        }
      }),
    );

    if (remainingTimes[player] <= 0) {
      TriggerLoss(player);
    }
  }

  function TriggerLoss(player: number) {
    // TODO
  }

  function StartTimer() {
    const countdownInterval = setInterval(() => {
      const currentTime = Date.now();
      const diff = (lastTickRef.current - currentTime) / 1000;
      lastTickRef.current = currentTime;

      IncrementPlayerTime(currentPlayer, diff);
    }, 100);

    return () => clearInterval(countdownInterval);
  }
  useEffect(StartTimer, [remainingTimes, currentPlayer]);

  function HandleMove(word: string) {
    word = word.toUpperCase();
    const maybeNext = graph.query(word);

    if (
      visitedRef.current.has(word) ||
      maybeNext == null ||
      !currentNode.children.has(maybeNext)
    ) {
      IncrementPlayerTime(currentPlayer, TIME_PENALTY);
      return;
    }

    visitedRef.current.add(word);

    IncrementPlayerTime(currentPlayer, TIME_GAIN);
    changeCurrentNode(maybeNext);
    changePreviousWord(currentWord);
    changeCurrentWord(word);
    changeCurrentPlayer((currentPlayer + 1) % 2);
  }

  // Load fonts
  const [fontsLoaded, fontsError] = useFonts({
    ChivoMono: require("../assets/fonts/ChivoMono-VariableFont_wght.ttf"),
    Chivo: require("../assets/fonts/Chivo-VariableFont_wght.ttf"),
  });

  useEffect(() => {
    if (fontsLoaded || fontsError) {
      SplashScreen.hideAsync();
    }
  }, [fontsLoaded, fontsError]);

  if (!fontsLoaded && !fontsError) {
    return null;
  }

  return (
    <View style={{ flex: 1, flexDirection: "column" }}>
      <View style={{ flex: 1, flexDirection: "row", gap: 100 }}>
        <Timer remainingTimes={remainingTimes} player={0} />
        <Timer remainingTimes={remainingTimes} player={1} />
      </View>
      <Flipboard currentWord={currentWord} previousWord={previousWord} />
      <TextInput
        editable
        onSubmitEditing={(e) => {
          HandleMove(e.nativeEvent.text);
        }}
      />
    </View>
  );
}
