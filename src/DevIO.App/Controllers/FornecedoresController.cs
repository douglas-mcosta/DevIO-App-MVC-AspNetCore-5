using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DevIO.App.DTO;
using DevIO.Business.Interfaces;
using AutoMapper;
using DevIO.Business.Models;
using System.Collections.Generic;
using DevIO.Business.Services;
using Microsoft.AspNetCore.Authorization;
using DevIO.App.Extensions;

namespace DevIO.App.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class FornecedoresController : BaseController
    {

        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IFornecedorService _fornecedorService;
        private readonly IMapper _mapper;
        private readonly INotificador _notificador;
        public FornecedoresController(IFornecedorRepository fornecedorRepository, IMapper mapper, IFornecedorService fornecedorService, INotificador notificador) : base(notificador)
        {
            _fornecedorRepository = fornecedorRepository;
            _mapper = mapper;
            _fornecedorService = fornecedorService;
            _notificador = notificador;
        }

        [AllowAnonymous]
        [Route("lista-de-fornecedores")]
        public async Task<IActionResult> Index()
        {
            return View(_mapper.Map<IEnumerable<FornecedorDTO>>(await _fornecedorRepository.ObterTodos()));
        }

        [AllowAnonymous]
        [Route("dados-do-fornecedor/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {

            var fornecedorDTO = await ObterFornecedorEndereco(id);
            if (fornecedorDTO == null)
            {
                return NotFound();
            }

            return View(fornecedorDTO);
        }

        [ClaimsAuthorize("Fornecedor","Criar")]
        [Route("novo-fornecedor")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("novo-fornecedor")]
        [ClaimsAuthorize("Fornecedor", "Criar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FornecedorDTO fornecedorDTO)
        {
            if (!ModelState.IsValid) return View(fornecedorDTO);

            await _fornecedorService.Adicionar(_mapper.Map<Fornecedor>(fornecedorDTO));
            if (!OperacaoValida()) return View(fornecedorDTO);
            return RedirectToAction(nameof(Index));

        }
        [Route("editar")]
        [ClaimsAuthorize("Fornecedor", "Editar")]
        public async Task<IActionResult> Edit(Guid id)
        {

            var fornecedorDTO = await ObterFornecedorProdutoEndereco(id);
            if (fornecedorDTO == null)
            {
                return NotFound();
            }
            return View(fornecedorDTO);
        }

        [HttpPost("editar")]
        [ClaimsAuthorize("Fornecedor", "Editar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, FornecedorDTO fornecedorDTO)
        {
            if (id != fornecedorDTO.Id) return NotFound();

            if (!ModelState.IsValid) return View(fornecedorDTO);

            await _fornecedorService.Atualizar(_mapper.Map<Fornecedor>(fornecedorDTO));
            if (!OperacaoValida()) return View(fornecedorDTO);

            return RedirectToAction(nameof(Index));
        }

        [Route("deletar")]
        [ClaimsAuthorize("Fornecedor", "Deletar")]
        public async Task<IActionResult> Delete(Guid id)
        {

            var fornecedorDTO = await ObterFornecedorEndereco(id);
            if (fornecedorDTO == null)
            {
                return NotFound();
            }

            return View(fornecedorDTO);
        }

        [ClaimsAuthorize("Fornecedor", "Deletar")]
        [HttpPost("deletar/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var fornecedorDTO = await ObterFornecedorEndereco(id);

            if (fornecedorDTO == null) NotFound();

            await _fornecedorService.Remover(id);
            if (!OperacaoValida()) return View(fornecedorDTO);


            return RedirectToAction(nameof(Index));
        }

        [Route("obter-endereco/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterEndereco(Guid id)
        {

            FornecedorDTO fornecedor = await ObterFornecedorEndereco(id);

            if (fornecedor == null) return NotFound();

            return PartialView("_EnderecoDetails", fornecedor);
        }
        [Route("atualizar-endereco/{id}:guid")]
        [ClaimsAuthorize("Fornecedor", "Editar")]
        public async Task<ActionResult> AtualizarEndereco(Guid id)
        {
            FornecedorDTO fornecedor = await ObterFornecedorEndereco(id);
            if (fornecedor == null) return NotFound();

            return PartialView("_AtualizarEndereco", new FornecedorDTO { Endereco = fornecedor.Endereco });
        }

        [HttpPost("atualizar-endereco")]
        [ValidateAntiForgeryToken]
        [ClaimsAuthorize("Fornecedor", "Editar")]
        public async Task<ActionResult> AtualizarEndereco(FornecedorDTO fornecedorDTO)
        {
            ModelState.Remove("Nome");
            ModelState.Remove("Documento");

            if (!ModelState.IsValid) return PartialView("_AtualizarEndereco", fornecedorDTO);

            await _fornecedorService.AtualizarEndereco(_mapper.Map<Endereco>(fornecedorDTO.Endereco));

            if (!OperacaoValida()) return PartialView("_AtualizarEndereco", fornecedorDTO.Endereco);

            var url = Url.Action("ObterEndereco","Fornecedores",new { id=fornecedorDTO.Endereco.FornecedorId });

            return Json(new { success = true, url });
        }

        [Route("obter-fornecedor-endereco/{fornecedorId:guid}")]
        private async Task<FornecedorDTO> ObterFornecedorEndereco(Guid fornecedorId)
        {
            return _mapper.Map<FornecedorDTO>(await _fornecedorRepository.ObterFornecedorEndereco(fornecedorId));
        }
        [Route("obter-fornecedor-produtos-endereco/{fornecedorId:guid}")]
        private async Task<FornecedorDTO> ObterFornecedorProdutoEndereco(Guid fornecedorId)
        {
            return _mapper.Map<FornecedorDTO>(await _fornecedorRepository.ObterFornecedorProdutosEndereco(fornecedorId));
        }
    }
}
