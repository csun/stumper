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

  // Possible Animations
  // - Anagram
  //  - All roll to next letter
  // - Single Edit
  //  - Single roll to next letter (if needed)
  // - Single Addition
  //  - Single open wipe
  // - Default case
  //  - Roll out to blank, roll in new

  // Maybe pass down to the letter what animation is desired (open wipe or roll from letter to letter)

  let letters = [];
  for (var i = 0; i < currentWord.length; i++) {
    const letter = currentWord[i];
    const key = i.toString() + letter;
    letters.push(
      <FlipboardLetter
        fontSize={fontSize}
        currentWord={letter}
        animType={"none"}
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
