# Greed Island Exercise No 7

## Quest 1

Refer to Game2048, find as much code smells as possible for each category.
It’s possible that there is no code smell for a category.
You can identify category by category or go through code top to bottom and identify code smell for each section.

1.	Duplicated code
2.	Deodorant comment
3.	Long method
4.	Dead code
5.	Lazy class
6.	Primitive obsession
7.	Switch statement
8.	Speculative generality 
9.	Feature envy
10.	Temporary field
11.	Inappropriate intimacy

Example   
Line 120  
private static bool Update(ulong[,] board, Direction direction, out ulong score)
Code smell: Long method
Explanation: The method is 90 lines, it might be doing many things which becomes difficult to understand and change

You also can use image for the code smell

## Quest 2
Please refactor and submit your repository