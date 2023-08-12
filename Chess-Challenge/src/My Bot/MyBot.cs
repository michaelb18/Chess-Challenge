using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections;
public class MyBot : IChessBot
{
    Hashtable transpositions = new Hashtable();
    private int is_white = 1;

    private double squareControl(ulong board)
    {   
        double score = 0;
        for(int x = 2; x < 6; x++)
        {
            for(int y = 2; y < 6; y++)
            {
                if(BitboardHelper.SquareIsSet(board, new Square(x, y)))
                    score += 1.0/(6.28) * Math.Exp(-((x - 4.5) * (x - 4.5) + (y - 4.5) * (y - 4.5))/2);
            }
        }
        return score;
    }

    public double evaluate(Board b)
    {
        //Avoid stalemate
        /*************************************************************/
        //Material Score
        int white_score = 0;
        int black_score = 0;
        PieceList white_pawns = b.GetPieceList(PieceType.Pawn, true);
        ulong whiteAttacks = 0;
        foreach(Piece pawn in white_pawns)
        {
            whiteAttacks = whiteAttacks | BitboardHelper.GetPieceAttacks(PieceType.Pawn, pawn.Square, b, true);
            white_score = white_score + 1;
        }

        PieceList white_bishop = b.GetPieceList(PieceType.Bishop, true);
        foreach(Piece bishop in white_bishop)
        {
            whiteAttacks = whiteAttacks | BitboardHelper.GetPieceAttacks(PieceType.Bishop, bishop.Square, b, true);
            white_score = white_score + 3;
        }

        PieceList white_knight = b.GetPieceList(PieceType.Knight, true);
        foreach(Piece knight in white_knight)
        {
            whiteAttacks = whiteAttacks | BitboardHelper.GetPieceAttacks(PieceType.Knight, knight.Square, b, true);
            white_score = white_score + 3;
        }

        PieceList white_queen = b.GetPieceList(PieceType.Queen, true);
        foreach(Piece queen in white_queen)
        {
            whiteAttacks = whiteAttacks | BitboardHelper.GetPieceAttacks(PieceType.Queen, queen.Square, b, true);
            white_score = white_score + 9;
        }

        PieceList white_king = b.GetPieceList(PieceType.King, true);
        foreach(Piece king in white_king)
        {
            //whiteAttacks = whiteAttacks | BitboardHelper.GetPieceAttacks(PieceType.King, king.Square, b, true);
            white_score = white_score + 10000;
        }

        ulong blackAttacks = 0;
        PieceList black_pawns = b.GetPieceList(PieceType.Pawn, false);
        foreach(Piece pawn in black_pawns)
        {
            blackAttacks = blackAttacks | BitboardHelper.GetPieceAttacks(PieceType.Pawn, pawn.Square, b, true);
            black_score = black_score + 1;
        }

        PieceList black_bishop = b.GetPieceList(PieceType.Bishop, false);
        foreach(Piece bishop in black_bishop)
        {
            blackAttacks = blackAttacks | BitboardHelper.GetPieceAttacks(PieceType.Bishop, bishop.Square, b, true);
            black_score = black_score + 3;
        }

        PieceList black_knight = b.GetPieceList(PieceType.Knight, false);
        foreach(Piece knight in black_knight)
        {
            blackAttacks = blackAttacks | BitboardHelper.GetPieceAttacks(PieceType.Knight, knight.Square, b, true);
            black_score = black_score + 3;
        }

        PieceList black_queen = b.GetPieceList(PieceType.Queen, false);
        foreach(Piece queen in black_queen)
        {
            blackAttacks = blackAttacks | BitboardHelper.GetPieceAttacks(PieceType.Queen, queen.Square, b, true);
            black_score = black_score + 9;
        }

        PieceList black_king = b.GetPieceList(PieceType.King, false);
        foreach(Piece king in white_king)
        {
            //blackAttacks = blackAttacks | BitboardHelper.GetPieceAttacks(PieceType.King, king.Square, b, true);
            black_score = black_score + 10000;
        }
        /*************************************************************/

        //double[] scoreVector = {(white_score - black_score) * is_white, (BitboardHelper.GetNumberOfSetBits(whiteAttacks) - BitboardHelper.GetNumberOfSetBits(blackAttacks)) * is_white};
        //double mag = Math.Sqrt(scoreVector[0] * scoreVector[0] + scoreVector[1] * scoreVector[1]);
        //scoreVector[0] /= mag;
        //scoreVector[1] /= mag;
        double[] scoreVector = {(white_score - black_score) * is_white, (squareControl(whiteAttacks) - squareControl(blackAttacks)) * is_white};
        double materialWeight = 0;
        return 1 * scoreVector[0] + 0 * scoreVector[1];
        //return (white_score - black_score) * is_white;
    }

    public (double score, Move m) minimax(int depth, Board b, bool max_or_min, double alpha, double beta)
    {
        if(b.IsDraw())
        {
            if(b.GetLegalMoves().Length>0)
            {
                return (-1000000000, b.GetLegalMoves()[0]);
            }
            else
            {
                return (-1000000000, Move.NullMove);
            }
        }
        if(depth == 0)
        {
            if(b.GetLegalMoves().Length>0)
            {
                return (evaluate(b), b.GetLegalMoves()[0]);
            }
            else
            {
                return (evaluate(b), Move.NullMove);
            }
        }
        if(max_or_min)
        {
            /*if(transpositions.ContainsKey(b.ZobristKey))
            {

                //Console.WriteLine(((Board)transpositions[b.ZobristKey]).ZobristKey);
                return minimax(depth, (Board)transpositions[b.ZobristKey], false, alpha, beta);
                //if(transpositions[b.ZobristKey] != null)
                    //b.MakeMove((Move)transpositions[b.ZobristKey]);
                    //return minimax(depth, b, false, alpha, beta);
            }*/

            Move bestMove = new Move();
            double max_score = -99999999;
            foreach(Move move in b.GetLegalMoves())
            {
                b.MakeMove(move);
                double score = minimax(depth-1, b, false, alpha, beta).Item1;
                //Console.WriteLine("Depth: " + depth + " Number of moves left: " + b.GetLegalMoves().Length + " This move's score: " + score);
                if(score > max_score)
                {
                    bestMove = move;
                    max_score = score;
                }
                b.UndoMove(move);
                alpha = Math.Max(alpha, max_score);
                if(beta <= alpha)
                {
                    /*b.MakeMove(bestMove);
                    transpositions[b.ZobristKey] = b;
                    b.UndoMove(bestMove);*/
                    return (max_score, bestMove);
                }
            }
            
            /*b.MakeMove(bestMove);
            transpositions[b.ZobristKey] = b;
            b.UndoMove(bestMove);*/
            return (max_score, bestMove);
        }
        else
        {
            /*if(transpositions.ContainsKey(b.ZobristKey))
            {
                //Console.WriteLine(((Board)transpositions[b.ZobristKey]).ZobristKey);
                //return minimax(depth, (Board)transpositions[b.ZobristKey], true, alpha, beta);
                //if(transpositions[b.ZobristKey] != null)
                //    b.MakeMove((Move)transpositions[b.ZobristKey]);
                //    return minimax(depth, b, true, alpha, beta);
            }*/
            Move bestMove = new Move();
            double min_score = 99999999;
            foreach(Move move in b.GetLegalMoves())
            {
                b.MakeMove(move);
                double score = minimax(depth-1, b, true, alpha, beta).Item1;
                //Console.WriteLine("Depth: " + depth + " Number of moves left: " + b.GetLegalMoves().Length + " This move's score: " + score);
                if(score < min_score)
                {
                    bestMove = move;
                    min_score = score;
                }
                b.UndoMove(move);
                beta = Math.Min(beta, min_score);
                if(beta <= alpha)
                {
                    /*b.MakeMove(bestMove);
                    transpositions[b.ZobristKey] = b;
                    b.UndoMove(bestMove);*/
                    return (min_score, bestMove);
                }
            }
            
            /*b.MakeMove(bestMove);
            transpositions[b.ZobristKey] = b;
            b.UndoMove(bestMove);*/
            
            return (min_score, bestMove);
        }
    }
    public Move Think(Board board, Timer timer)
    {
        if(!board.IsWhiteToMove)
        {
            is_white = -1;
        }
        int ms_total = timer.MillisecondsRemaining;
        double score = 0;
        Move m = board.GetLegalMoves()[0];
        int iter = 1;
        double alpha = double.NegativeInfinity;
        double beta = double.PositiveInfinity;
        while(timer.MillisecondsElapsedThisTurn < (int)(.01 * ms_total))
        {
            (score, m) = minimax(iter, board, true, alpha, beta);
            Console.WriteLine("Score is " + score + " at iteration " + iter);
            iter++;
        }
        Console.WriteLine(score);
        if(m == new Move())
        {
            return board.GetLegalMoves()[0];
        }
        return m;
    }
}