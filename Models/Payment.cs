using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace web3father_CSharp.Models
{
    public class Payment
    {
        public int Id { get; set; } // ID do pagamento (gerado automaticamente)
        public long UserId { get; set; } // ID do usuário associado ao pagamento
        public string PaymentId { get; set; } // Identificador único do pagamento
        public string PaymentDate { get; set; } // Data em que o pagamento foi efetuado
        public string DueDate { get; set; } // Data de vencimento do pagamento
    }
}
