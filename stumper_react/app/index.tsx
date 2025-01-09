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

  return (
    <View>
      <TextInput
        editable
        onChangeText={(text) => changeCurrentWord(text)}
        value={currentWord}
      />
      <TextInput
        editable
        onChangeText={(text) => changePreviousWord(text)}
        value={previousWord}
      />
      <Flipboard currentWord={currentWord} previousWord={previousWord} />
    </View>
  );
}
