﻿using Newtonsoft.Json.Linq;
using SanTsgProje.Application.Interfaces;
using SanTsgProje.Application.Models;
using SanTsgProje.Data;
using SanTsgProje.Domain.Authentications;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SanTsgProje.Application.Services
{
    
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthenticationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Authentication() // Authentication for connect Api's with Token
        {
            //Url for post api
            var url = "http://service.stage.paximum.com/v2/api/authenticationservice/login";
            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };
            //Post with httpclient 
            var httpClient = new HttpClient();
            var person = new AuthenticationRequest() { Agency = "PXM25397", User = "USR1", Password = "test!23" };
            var response = await httpClient.PostAsJsonAsync(url, person);
            //If response is success filter to Token and Token Expires On
            if (response.IsSuccessStatusCode)
            {
                TokenInfo token = new TokenInfo();
                var id = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(id);
                token.Token = json.SelectToken("body.token").Value<string>();
                token.ExpiresOn = json.SelectToken("body.expiresOn").Value<DateTime>();
                _unitOfWork.Authentication.Add(token);
                _unitOfWork.Complete();
            }
        }
        public bool IsTokenExpired() // Check it token expires On 
        {
            var token = _unitOfWork.Authentication.GetById();
            var now = DateTime.Now;
            var expireDay = (token.ExpiresOn.Day) - (now.Day);
            var expireHour = (token.ExpiresOn.Hour) - (now.Hour);
            // if token has expired return true or not return false
            if (expireDay < 0 && expireHour < 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

    }
}