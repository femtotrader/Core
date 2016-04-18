#region License
/*
Copyright Quantler BV, based on original code copyright Tradelink.org. 
This file is released under the GNU Lesser General Public License v3. http://www.gnu.org/copyleft/lgpl.html

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.
*/
#endregion

using Quantler.Interfaces;
using System;

namespace Quantler.Broker
{
    /// <summary>
    /// Orderbook representation
    /// </summary>
    public struct Book : GotTickIndicator
    {
        #region Public Fields

        public const string Emptyrequestor = "EMPTY";
        public const int Maxbook = 40;
        public int ActualDepth;

        public string[] Askex;

        public decimal[] Askprice;

        public int[] Asksize;

        public string[] Bidex;

        public decimal[] Bidprice;

        public int[] Bidsize;

        public string Sym;

        public int UpdateTime;

        #endregion Public Fields

        #region Private Fields

        private readonly int _maxbook;

        #endregion Private Fields

        #region Public Constructors

        public Book(Book b)
        {
            ActualDepth = b.ActualDepth;
            UpdateTime = b.UpdateTime;
            Sym = b.Sym;
            _maxbook = b._maxbook;
            Bidprice = new decimal[b.Askprice.Length];
            Bidsize = new int[b.Askprice.Length];
            Askprice = new decimal[b.Askprice.Length];
            Asksize = new int[b.Askprice.Length];
            Bidex = new string[b.Askprice.Length];
            Askex = new string[b.Askprice.Length];
            Array.Copy(b.Bidprice, Bidprice, b.Bidprice.Length);
            Array.Copy(b.Bidsize, Bidsize, b.Bidprice.Length);
            Array.Copy(b.Askprice, Askprice, b.Bidprice.Length);
            Array.Copy(b.Asksize, Asksize, b.Bidprice.Length);
            for (int i = 0; i < b.ActualDepth; i++)
            {
                Bidex[i] = b.Bidex[i];
                Askex[i] = b.Askex[i];
            }
        }

        public Book(string sym)
        {
            UpdateTime = 0;
            ActualDepth = 0;
            _maxbook = Maxbook;
            Sym = sym;
            Bidprice = new decimal[_maxbook];
            Bidsize = new int[_maxbook];
            Askprice = new decimal[_maxbook];
            Asksize = new int[_maxbook];
            Bidex = new string[_maxbook];
            Askex = new string[_maxbook];
        }

        #endregion Public Constructors

        #region Public Properties

        public bool IsValid { get { return Sym != null; } }

        #endregion Public Properties

        #region Public Methods

        public static string NewDOMRequest(int depthrequested)
        {
            return NewDOMRequest(Emptyrequestor, depthrequested);
        }

        public static string NewDOMRequest(string client, int depthrequested)
        {
            return string.Join("+", client, depthrequested.ToString());
        }

        public static bool ParseDomRequest(string request, ref int depth, ref string client)
        {
            string[] r = request.Split('+');
            if (r.Length != 2) return false;
            if (!int.TryParse(r[1], out depth))
                return false;
            client = r[0];
            return true;
        }

        public void GotTick(Tick k)
        {
            // ignore trades
            if (k.IsTrade) return;
            // make sure depth is valid for this book
            if ((k.Depth < 0) || (k.Depth >= _maxbook)) return;
            if (Sym == null)
                Sym = k.Symbol;
            // make sure symbol matches
            if (k.Symbol != Sym) return;
            // if depth is zero, must be a new book
            if (k.Depth == 0) Reset();
            // update buy book
            if (k.HasBid)
            {
                Bidprice[k.Depth] = k.Bid;
                Bidsize[k.Depth] = k.BidSize;
                Bidex[k.Depth] = k.BidExchange;
                if (k.Depth > ActualDepth)
                    ActualDepth = k.Depth;
            }
            // update sell book
            if (k.HasAsk)
            {
                Askprice[k.Depth] = k.Ask;
                Asksize[k.Depth] = k.AskSize;
                Askex[k.Depth] = k.AskExchange;
                if (k.Depth > ActualDepth)
                    ActualDepth = k.Depth;
            }
        }

        public void Reset()
        {
            ActualDepth = 0;
            for (int i = 0; i < _maxbook; i++)
            {
                Bidex[i] = null;
                Askex[i] = null;
                Bidprice[i] = 0;
                Bidsize[i] = 0;
                Askprice[i] = 0;
                Asksize[i] = 0;
            }
        }

        #endregion Public Methods
    }
}