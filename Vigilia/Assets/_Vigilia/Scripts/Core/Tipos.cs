// Enums compartilhados. Nomes seguem o glossário do GDD (docs/gdd/04-glossario.md).
namespace Vigilia
{
    /// <summary>Atributo de cada sala. Multiplica a vazão base da possessão (GDD 6.3).</summary>
    public enum TemperaturaSala { Fria, Morna, Quente }

    /// <summary>Os três finais (GDD 11). O final C tem duas variações, decididas pelo EndingResolver.</summary>
    public enum FinalMateria { Honesta, Omite, Louco }

    /// <summary>Máquina macro, o fim de semana (GDD 9.1). Estados iguais ao Apêndice A.</summary>
    public enum EstadoMacro { Intro, Dia1, Noite1, Dia2, Noite2, Dia3, Noite3, FinalA, FinalB, FinalC }

    /// <summary>Jogador dentro de uma noite (GDD 9.2).</summary>
    public enum EstadoJogador { Explorando, Perseguido, Escondido, Tocado, NoQuarto, Escrevendo, Dormindo, Transbordou }

    /// <summary>Inimigo (GDD 9.3). Rotina e Seguindo são de dia; o resto é de noite.</summary>
    public enum EstadoInimigo { Rotina, Seguindo, Patrulha, Perseguindo, Congelado, Atordoado, Recuando, Morto }

    /// <summary>tipoFuncionario e tipoPacienteVelho do glossário.</summary>
    public enum TipoInimigo { Funcionario, PacienteVelho }

    /// <summary>Tudo que o jogador pode pegar no mapa.</summary>
    public enum TipoItem { Pilha, Cobertor, Chave, PeDeCabra, Evidencia, NotaPropria, Pulseira, PistaDiretor, Sedativo, ObjetoPesado }
}
