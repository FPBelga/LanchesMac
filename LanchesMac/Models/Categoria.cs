namespace LanchesMac.Models
{
    public class Categoria
    {
        public int CategoriaÍd { get; set; }
        public string CategoriaNome { get; set; }
        public string Descricao { get; set; }
        public List<Lanche> Lanches { get; set; }
    }

}
