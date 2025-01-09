import { View, StyleSheet, useWindowDimensions } from "react-native";
import FlipboardLetter from "./flipboard_letter";

export default function Flipboard({
  currentWord,
  previousWord,
}: {
  currentWord: string;
  previousWord: string;
}) {
  const fontSize = useWindowDimensions().width / 12;

  let letters = [];
  for (var i = 0; i < currentWord.length; i++) {
    const letter = currentWord[i];
    const key = i.toString() + letter;
    letters.push(
      <FlipboardLetter
        fontSize={fontSize}
        currentWord={letter}
        previousWord=""
        key={key}
      >
        {letter}
      </FlipboardLetter>,
    );
  }

  return (
    <View
      style={[
        styles.container,
        {
          gap: fontSize / 8,
        },
      ]}
    >
      {letters}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    flexDirection: "row",
    justifyContent: "center",
    alignItems: "center",
  },
});
