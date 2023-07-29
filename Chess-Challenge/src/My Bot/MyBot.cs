using ChessChallenge.API;
using System;
public class MyBot : IChessBot
{

    int[] pieceValues = { 100, 300, 300, 500, 900, 10000 };
    private int is_white = 1;
    public double evaluate(Board b)
    {
        int white_score = 0;
        int black_score = 0;

        PieceList white_pawns = b.GetPieceList(PieceType.Pawn, true);
        foreach(Piece pawn in white_pawns)
        {
            white_score = white_score + 1;
        }

        PieceList white_bishop = b.GetPieceList(PieceType.Bishop, true);
        foreach(Piece bishop in white_bishop)
        {
            white_score = white_score + 3;
        }

        PieceList white_knight = b.GetPieceList(PieceType.Knight, true);
        foreach(Piece knight in white_knight)
        {
            white_score = white_score + 3;
        }

        PieceList white_queen = b.GetPieceList(PieceType.Queen, true);
        foreach(Piece queen in white_queen)
        {
            white_score = white_score + 9;
        }

        PieceList white_king = b.GetPieceList(PieceType.King, true);
        foreach(Piece king in white_king)
        {
            white_score = white_score + 999999;
        }

        PieceList black_pawns = b.GetPieceList(PieceType.Pawn, false);
        foreach(Piece pawn in black_pawns)
        {
            black_score = black_score + 1;
        }

        PieceList black_bishop = b.GetPieceList(PieceType.Bishop, false);
        foreach(Piece bishop in black_bishop)
        {
            black_score = black_score + 3;
        }

        PieceList black_knight = b.GetPieceList(PieceType.Knight, false);
        foreach(Piece knight in black_knight)
        {
            black_score = black_score + 3;
        }

        PieceList black_queen = b.GetPieceList(PieceType.Queen, false);
        foreach(Piece queen in black_queen)
        {
            black_score = black_score + 9;
        }

        PieceList black_king = b.GetPieceList(PieceType.King, false);
        foreach(Piece king in white_king)
        {
            black_score = black_score + 999999;
        }

        return (white_score - black_score) * is_white;
    }

    public (double score, Move m) minimax(int depth, Board b, bool max_or_min, double alpha, double beta)
    {
        if(depth == 0)
        {
            if(b.GetLegalMoves().Length>0)
            {
                return (evaluate(b), b.GetLegalMoves()[0]);
            }
            else
            {
                return (evaluate(b), new Move());
            }
        }
        if(max_or_min)
        {
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
                    break;
                }
            }
            return (max_score, bestMove);
        }
        else
        {
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
                    break;
                }
            }
            return (min_score, bestMove);
        }
    }
    public Move Think(Board board, Timer timer)
    {
        if(!board.IsWhiteToMove)
        {
            is_white = -1;
        }
        return minimax(6, board, true, double.NegativeInfinity, double.PositiveInfinity).m;
    }
}