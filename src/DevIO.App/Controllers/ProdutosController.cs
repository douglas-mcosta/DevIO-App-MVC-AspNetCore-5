using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DevIO.App.DTO;
using DevIO.Business.Interfaces;
using AutoMapper;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using DevIO.Business.Services;
using Microsoft.AspNetCore.Authorization;
using DevIO.App.Extensions;

namespace DevIO.App.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ProdutosController : BaseController
    {

        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly INotificador _notificador;
        private readonly IMapper _mapper;

        public ProdutosController(IFornecedorRepository fornecedorRepository, IProdutoRepository produtoRepository, IMapper mapper, IProdutoService produtoService, INotificador notificador) : base(notificador)
        {
            _fornecedorRepository = fornecedorRepository;
            _produtoRepository = produtoRepository;
            _mapper = mapper;
            _produtoService = produtoService;
            _notificador = notificador;
        }

        [AllowAnonymous]
        [Route("lista-de-produtos")]
        public async Task<IActionResult> Index()
        {

            return View(_mapper.Map<IEnumerable<ProdutoDTO>>(await _produtoRepository.ObterProdutosFornecedores()));
        }
        [AllowAnonymous]
        [Route("detalhes/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var produtoDTO = await ObterProduto(id);

            if (produtoDTO == null)
            {
                return NotFound();
            }

            return View(produtoDTO);
        }

        [ClaimsAuthorize("Produto", "Criar")]
        [Route("Cadastrar")]
        public async Task<IActionResult> Create()
        {
            var ProdutoDTO = await PopulaFornecedor(new ProdutoDTO());
            return View(ProdutoDTO);
        }

        [HttpPost("cadastrar")]
        [ClaimsAuthorize("Produto", "Criar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProdutoDTO produtoDTO)
        {
            produtoDTO = await PopulaFornecedor(produtoDTO);
            string preFixo = Guid.NewGuid() + "-";
            if (!ModelState.IsValid) return View(produtoDTO);

            if (!await UploadArquivo(produtoDTO.ImagemUpload, preFixo))
            {
                return View(produtoDTO);
            }
            produtoDTO.Imagem = preFixo + produtoDTO.ImagemUpload.FileName;
            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoDTO));

            if (!OperacaoValida()) return View(produtoDTO);

            return RedirectToAction(nameof(Index));
        }
        [Route("editar/{id:guid}")]
        [ClaimsAuthorize("Produto", "Editar")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var produtoDTO = await ObterProduto(id);
            if (produtoDTO == null) return NotFound();

            return View(produtoDTO);
        }

        [ClaimsAuthorize("Produto", "Editar")]
        [HttpPost("editar/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProdutoDTO produtoDTO)
        {
            if (id != produtoDTO.Id) return NotFound();

            var produtoAtualizacao = await ObterProduto(id);
            produtoDTO.Fornecedor = produtoAtualizacao.Fornecedor;
            produtoDTO.Imagem = produtoAtualizacao.Imagem;
            if (!ModelState.IsValid) return View(produtoDTO);

            if (produtoDTO.ImagemUpload != null)
            {
                string preFixo = Guid.NewGuid() + "-";
                if (!await UploadArquivo(produtoDTO.ImagemUpload, preFixo))
                {
                    return View(produtoDTO);
                }
                produtoAtualizacao.Imagem = preFixo + produtoDTO.ImagemUpload.FileName;
            }
            produtoAtualizacao.Nome = produtoDTO.Nome;
            produtoAtualizacao.Descricao = produtoDTO.Descricao;
            produtoAtualizacao.Valor = produtoDTO.Valor;
            produtoAtualizacao.Ativo = produtoDTO.Ativo;

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizacao));
            if (!OperacaoValida()) return View(produtoDTO);

            return RedirectToAction(nameof(Index));
        }

        [ClaimsAuthorize("Produto", "Excluir")]
        [Route("deletar/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var produto = await ObterProduto(id);
            if (produto == null) return NotFound();

            return View(produto);
        }

        [ClaimsAuthorize("Produto", "Excluir")]
        [Route("deletar/{id:guid}")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var produto = ObterProduto(id);
            if (produto == null) return NotFound();

            await _produtoService.Remover(id);
            if (!OperacaoValida()) return View(produto);

            return RedirectToAction(nameof(Index));
        }

        [Route("obter-produto/{fornecedorId:guid}")]
        private async Task<ProdutoDTO> ObterProduto(Guid fornecedorId)
        {

            var produto = _mapper.Map<ProdutoDTO>(await _produtoRepository.ObterProdutoFornecedor(fornecedorId));
            produto.Fornecedores = _mapper.Map<IEnumerable<FornecedorDTO>>(await _fornecedorRepository.ObterTodos());
            return produto;
        }
        [Route("popula-fornecedor")]
        private async Task<ProdutoDTO> PopulaFornecedor(ProdutoDTO produto)
        {
            produto.Fornecedores = _mapper.Map<IEnumerable<FornecedorDTO>>(await _fornecedorRepository.ObterTodos());
            return produto;
        }
        [Route("upload-de-arquivo")]
        private async Task<bool> UploadArquivo(IFormFile file, string preFixo)
        {

            if (file.Length <= 0) return false;

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", preFixo + file.FileName);

            if (System.IO.File.Exists(path))
            {
                ModelState.AddModelError(string.Empty, "Já existe um arquivo com esse nome");
                return false;
            }

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return true;
        }
    }
}
