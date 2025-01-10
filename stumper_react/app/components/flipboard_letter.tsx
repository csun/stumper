import { Animated, Text, View, StyleSheet } from "react-native";
import { ReactNode, useEffect, useState } from "react";

export enum AnimType {
  NONE = 0,
  OPEN_WIPE,
  ROLL_IN,
}

export default function FlipboardLetter({
  fontSize,
  currentWord,
  animType,
  previousWord,
  children,
}: {
  fontSize: number;
  currentWord: string;
  animType: AnimType;
  previousWord?: string;
  children?: ReactNode;
}) {
  const styles = StyleSheet.create({
    letters: {
      fontFamily: "ChivoMono",
      fontSize: fontSize,
      fontWeight: "600",
      textAlign: "center",
    },
  });

  if (animType == AnimType.NONE) {
    return (
      <View>
        <Text style={styles.letters}>{currentWord}</Text>
      </View>
    );
  }

  if (animType == AnimType.OPEN_WIPE) {
    const growAnim = new Animated.Value(0);

    useEffect(() => {
      Animated.timing(growAnim, {
        toValue: fontSize,
        duration: 500,
        useNativeDriver: true,
      }).start();
    }, [growAnim]);

    return (
      <Animated.View style={{ maxWidth: growAnim, overflow: "hidden" }}>
        <Text style={styles.letters}>{currentWord}</Text>
      </Animated.View>
    );
  } else {
  }
}
