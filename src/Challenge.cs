// DESAFIO: Integração com Sistema Legado de Pagamentos
// PROBLEMA: Um e-commerce moderno precisa integrar com um sistema legado de processamento
// de pagamentos que usa interfaces e estruturas de dados incompatíveis com o sistema atual
// O código atual não consegue usar o sistema legado sem grandes mudanças na aplicação

using System;

namespace DesignPatternChallenge
{
    // Contexto: Sistema moderno de e-commerce com interface padronizada
    // Precisa integrar com sistema legado que tem interface completamente diferente
    
    // Interface moderna que a aplicação usa
    public interface IPaymentProcessor
    {
        PaymentResult ProcessPayment(PaymentRequest request);
        bool RefundPayment(string transactionId, decimal amount);
        PaymentStatus CheckStatus(string transactionId);
    }

    public class PaymentRequest
    {
        public string CustomerEmail { get; set; }
        public decimal Amount { get; set; }
        public string CreditCardNumber { get; set; }
        public string Cvv { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Description { get; set; }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Approved,
        Declined,
        Refunded
    }

    // Sistema legado com interface completamente diferente
    public class LegacyPaymentSystem
    {
        // Métodos com assinaturas incompatíveis
        public LegacyTransactionResponse AuthorizeTransaction(
            string cardNum,
            int cvvCode,
            int expMonth,
            int expYear,
            double amountInCents,
            string customerInfo)
        {
            Console.WriteLine($"[Sistema Legado] Autorizando transação...");
            Console.WriteLine($"Cartão: {cardNum}");
            Console.WriteLine($"Valor: {amountInCents / 100:C}");
            
            // Simulação de processamento
            var response = new LegacyTransactionResponse
            {
                AuthCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                ResponseCode = "00",
                ResponseMessage = "TRANSACTION APPROVED",
                TransactionRef = $"LEG{DateTime.Now.Ticks}"
            };

            return response;
        }

        public bool ReverseTransaction(string transRef, double amountInCents)
        {
            Console.WriteLine($"[Sistema Legado] Revertendo transação {transRef}");
            Console.WriteLine($"Valor: {amountInCents / 100:C}");
            return true;
        }

        public string QueryTransactionStatus(string transRef)
        {
            Console.WriteLine($"[Sistema Legado] Consultando transação {transRef}");
            return "APPROVED";
        }
    }

    public class LegacyTransactionResponse
    {
        public string AuthCode { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string TransactionRef { get; set; }
    }

    // Implementação moderna que funciona bem
    public class ModernPaymentProcessor : IPaymentProcessor
    {
        public PaymentResult ProcessPayment(PaymentRequest request)
        {
            Console.WriteLine("[Processador Moderno] Processando pagamento...");
            return new PaymentResult
            {
                Success = true,
                TransactionId = Guid.NewGuid().ToString(),
                Message = "Pagamento aprovado"
            };
        }

        public bool RefundPayment(string transactionId, decimal amount)
        {
            Console.WriteLine($"[Processador Moderno] Reembolsando {amount:C}");
            return true;
        }

        public PaymentStatus CheckStatus(string transactionId)
        {
            return PaymentStatus.Approved;
        }
    }

    // Classe da aplicação que usa a interface moderna
    public class CheckoutService
    {
        private readonly IPaymentProcessor _paymentProcessor;

        public CheckoutService(IPaymentProcessor paymentProcessor)
        {
            _paymentProcessor = paymentProcessor;
        }

        public void CompleteOrder(string customerEmail, decimal amount, string cardNumber)
        {
            Console.WriteLine($"\n=== Finalizando Pedido ===");
            Console.WriteLine($"Cliente: {customerEmail}");
            Console.WriteLine($"Valor: {amount:C}\n");

            var request = new PaymentRequest
            {
                CustomerEmail = customerEmail,
                Amount = amount,
                CreditCardNumber = cardNumber,
                Cvv = "123",
                ExpirationDate = new DateTime(2026, 12, 31),
                Description = "Compra de produtos"
            };

            var result = _paymentProcessor.ProcessPayment(request);

            if (result.Success)
            {
                Console.WriteLine($"✅ Pedido aprovado! ID: {result.TransactionId}");
            }
            else
            {
                Console.WriteLine($"❌ Pagamento recusado: {result.Message}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Checkout ===\n");

            // Funciona bem com o processador moderno
            var modernProcessor = new ModernPaymentProcessor();
            var checkoutWithModern = new CheckoutService(modernProcessor);
            checkoutWithModern.CompleteOrder("cliente@email.com", 150.00m, "4111111111111111");

            Console.WriteLine("\n" + new string('-', 60) + "\n");

            // Problema: Como usar o sistema legado sem modificar CheckoutService?
            var legacySystem = new LegacyPaymentSystem();
            
            // ISSO NÃO FUNCIONA - Interfaces incompatíveis
            // var checkoutWithLegacy = new CheckoutService(legacySystem); // ERRO DE COMPILAÇÃO!

            Console.WriteLine("⚠️ PROBLEMA: Sistema legado não implementa IPaymentProcessor");
            Console.WriteLine("   - Assinaturas de métodos incompatíveis");
            Console.WriteLine("   - Estruturas de dados diferentes");
            Console.WriteLine("   - Não podemos modificar o código legado");
            Console.WriteLine("   - Não queremos modificar CheckoutService");

            // Tentativa ingênua: criar wrapper manualmente em cada lugar
            Console.WriteLine("\n--- Tentativa de uso direto (código duplicado) ---\n");
            
            var cardNumber = "4111111111111111";
            var cvv = 123;
            var expDate = new DateTime(2026, 12, 31);
            var amount = 200.00m;

            // Conversões manuais repetidas em cada lugar do código
            var legacyResponse = legacySystem.AuthorizeTransaction(
                cardNumber,
                cvv,
                expDate.Month,
                expDate.Year,
                (double)(amount * 100),
                "cliente2@email.com"
            );

            if (legacyResponse.ResponseCode == "00")
            {
                Console.WriteLine($"✅ Transação aprovada! Ref: {legacyResponse.TransactionRef}");
            }

            // Perguntas para reflexão:
            // - Como fazer o sistema legado trabalhar com a interface moderna?
            // - Como evitar modificar CheckoutService e outras classes que usam IPaymentProcessor?
            // - Como encapsular as conversões entre as interfaces incompatíveis?
            // - Como permitir que ambos os sistemas coexistam de forma transparente?
        }
    }
}
