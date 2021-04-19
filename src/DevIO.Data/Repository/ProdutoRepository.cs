﻿using DevIO.Business.Interfaces;
using DevIO.Business.Models;
using DevIO.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevIO.Data.Repository
{
    public class ProdutoRepository : Repository<Produto>, IProdutoRepository
    {

        public ProdutoRepository(MeuDbContext context) : base(context) {}
        public async Task<Produto> ObterProdutoFornecedor(Guid id) => await DbSet.AsNoTracking().Include(f=>f.Fornecedor).FirstOrDefaultAsync(p=>p.Id == id);


        public async Task<IEnumerable<Produto>> ObterProdutosFornecedores()
        {
            return await DbSet.AsNoTracking().Include(f => f.Fornecedor).ToListAsync();
        }
        public async Task<IEnumerable<Produto>> ObterProdutoPorFornecedor(Guid fornecedorId)
        {
            return await Buscar(p => p.FornecedorId == fornecedorId);
        }
    }
}