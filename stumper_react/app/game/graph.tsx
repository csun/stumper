export type Node = {
  word: string;
  anagramKey: string;
  logFrequency: number;
  children: Set<Node>;
};
