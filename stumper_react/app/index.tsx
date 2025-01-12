import { useFonts } from "expo-font";
import * as SplashScreen from "expo-splash-screen";
import { useEffect, useState } from "react";
import { Text, View, StyleSheet, TextInput } from "react-native";
import Flipboard from "./components/flipboard";

SplashScreen.preventAutoHideAsync();

export default function Index() {
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

  const [currentWord, changeCurrentWord] = useState("STUMPER");
  const [previousWord, changePreviousWord] = useState("STMPER");

  function HandleMove(word: string) {
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
