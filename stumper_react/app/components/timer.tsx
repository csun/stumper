import { View, Text, StyleSheet, useWindowDimensions } from "react-native";
import FlipboardLetter, { AnimType } from "./flipboard_letter";

const styles = StyleSheet.create({
  letters: {
    fontFamily: "ChivoMono",
    fontSize: 20,
    fontWeight: "600",
    textAlign: "center",
  },
});

export default function Timer({
  remainingTimes,
  player,
}: {
  remainingTimes: number[];
  player: number;
}) {
  return (
    <View>
      <Text style={styles.letters}>{Math.ceil(remainingTimes[player])}</Text>
    </View>
  );
}
