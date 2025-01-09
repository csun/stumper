import { Animated, Text, View, StyleSheet } from "react-native";
import { ReactNode, useEffect } from "react";

export default function FlipboardLetter({
  children,
  fontSize,
  currentWord,
  previousWord,
}: {
  children?: ReactNode;
  fontSize: number;
  currentWord: string;
  previousWord: string;
}) {
  const styles = StyleSheet.create({
    letters: {
      fontFamily: "ChivoMono",
      fontSize: fontSize,
      fontWeight: 600,
    },
  });

  if (previousWord.length == 0) {
    const growAnim = new Animated.Value(0);
    useEffect(() => {
      Animated.timing(growAnim, {
        toValue: 63,
        duration: 1000,
        useNativeDriver: true,
      }).start();
    }, [growAnim]);

    return (
      <Animated.View style={{ flexBasis: growAnim, overflow: "hidden" }}>
        <Text style={styles.letters}>{currentWord}</Text>
      </Animated.View>
    );
  } else {
    return (
      <View>
        <Text style={styles.letters}>{currentWord}</Text>
      </View>
    );
  }
}
