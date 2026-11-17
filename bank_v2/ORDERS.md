# Bank v2: orders

Orders only, one wording per item (level 0, robotic), no clarification items. Mix per log: filters 2 act / 2 var / 2 time / 2 mixed; circuits 4 act / 4 var / 2 time / 2 mixed; regions 5 act / 3 time / 2 var; milestones activities only. Gold verified with the PMAppDeclarativeEngine (`--verify-bank`, 0 failures in the 17 logs). The class of each item is stored in `cls`.

| Log | lang | cases | filters | circuits | regions | milestones | total |
|---|---|---|---|---|---|---|---|
| atencion_primaria | es | 2000 | 8 | 12 | 10 | 10 | 40 |
| bpic2012 | en | 13087 | 8 | 12 | 10 | 10 | 40 |
| bpic2017 | en | 31509 | 8 | 12 | 10 | 10 | 40 |
| bpic2019 | en | 251734 | 8 | 12 | 10 | 10 | 40 |
| bpic2020_domestic_declarations | en | 10500 | 8 | 12 | 10 | 10 | 40 |
| bpic2020_international_declarations | en | 6449 | 8 | 12 | 10 | 10 | 40 |
| bpic2020_prepaid_travel_costs | en | 2099 | 8 | 12 | 10 | 10 | 40 |
| bpic2020_request_for_payment | en | 6886 | 8 | 12 | 10 | 10 | 40 |
| bpic2020_travel_permit | en | 7065 | 8 | 12 | 10 | 10 | 40 |
| dolor_toracico_urgencias | es | 2000 | 8 | 12 | 10 | 10 | 40 |
| gestion_citas | es | 2000 | 8 | 12 | 10 | 10 | 40 |
| helpdesk | en | 4580 | 8 | 12 | 10 | 10 | 40 |
| hospital_billing | en | 100000 | 8 | 12 | 10 | 10 | 40 |
| ictus | es | 2000 | 8 | 12 | 10 | 10 | 40 |
| road_traffic_fine | en | 150370 | 8 | 12 | 10 | 10 | 40 |
| sepsis_cases | en | 1050 | 8 | 12 | 10 | 10 | 40 |
| sepsis_hospital_complejo | es | 2000 | 8 | 12 | 10 | 10 | 40 |

## atencion_primaria

### Filters

* **F01** [act] Limita los casos a: casos en los que ocurre «AtencionUrgenciasHospital»
* **F02** [act] Limita los casos a: pacientes/casos que pasaron por «SolicitudRetinografia»
* **F32** [var] Limita los casos a: casos con IMC mayor que 26.6
* **F33** [var] Limita los casos a: casos con IMC entre 26.6 y 31.6
* **F39** [time] Limita los casos a: casos con «Analitica» y, más tarde, «IntensificacionAntidiabeticos» en menos de 5 días
* **F40** [time] Limita los casos a: casos con «ConsultaMedicaResultados» y, más tarde, «ObjetivoLDLNoAlcanzado» con más de 30 días entre ambos
* **F38** [mixed] Limita los casos a: casos con «RegistroProgramaLipidos» y, más tarde, «RevisionAnualCronicos», con ExploracionPieHecha igual a «si»
* **F42** [mixed] Limita los casos a: casos con IMC mayor que 27.3 y «RevisionAnualCronicos» en menos de 14 días desde «RegistroProgramaLipidos»

### Circuits

* **C01** [act] Crea circuitos para esto: separa a los pacientes por cómo acaba su año: seguimiento completo, abandono, fallecimiento en casa o fallecimiento en el hospital
* **C02** [act] Crea circuitos para esto: clasifica a los pacientes según hayan pasado por urgencias del hospital, hayan pedido retinografía o hayan recibido educación diabetológica (el primero que cumpla)
* **C03** [act] Crea circuitos para esto: separa los pacientes que pasaron del programa de lípidos a la revisión anual, los que solo están en el programa de lípidos y el resto
* **C04** [act] Crea circuitos para esto: separa los casos según cómo terminan: «FinSeguimientoCompleto» o «AbandonoSeguimiento»
* **C05** [var] Crea circuitos para esto: separa a los diabéticos que se hicieron la exploración del pie de los que no
* **C06** [var] Crea circuitos para esto: agrupa los casos por AccesoDigital: «si» o «no»
* **C07** [var] Crea un circuito por cada valor de Adherencia: «buena» o «regular»
* **C08** [var] Crea circuitos para esto: separa los casos según AnaliticaRepetida: «no» o «si»
* **C09** [time] Crea tres circuitos: «AplazaDecisionTratamiento» y luego «EvaluacionAnualIndicadores» en menos de 30 días, el resto de los que tienen «AplazaDecisionTratamiento» y luego «EvaluacionAnualIndicadores», y los demás
* **C10** [time] Crea tres circuitos: «RegistroProgramaLipidos» y luego «Espirometria» en menos de 14 días, el resto de los que tienen «RegistroProgramaLipidos» y luego «Espirometria», y los demás
* **C11** [mixed] Crea tres circuitos: casos con AccesoDigital igual a «si» que pasan por «EvaluacionAnualIndicadores», el resto de casos con AccesoDigital igual a «si», y los demás
* **C12** [mixed] Haz tres circuitos: Adherencia igual a «buena» y «RegistroProgramaHipertension», solo Adherencia igual a «buena», y el resto

### Regions

* **R01** [act] Define una región desde «RegistroProgramaLipidos» hasta «RevisionAnualCronicos»
* **R02** [act] Define una sección crítica desde «SolicitudCitaPresencial» hasta «CitaDemorada»
* **R03** [act] Define una región desde «CitaDemorada» hasta «EducacionDiabetologica»
* **R04** [act] Define dos secciones críticas: de «ExploracionPie» a «ObjetivoTANoAlcanzado» y de «InicioSeguimiento» a «AnaliticaRepetida»
* **R05** [act] Define dos regiones: de «CitaDemorada» a «EducacionDiabetologica» y de «RevisionAnualCronicos» a «ObjetivoTAAlcanzado»
* **R06** [time] Define una región desde «AplazaDecisionTratamiento» hasta «EvaluacionAnualIndicadores» cuando pasan menos de 30 días entre ambos
* **R07** [time] Define una sección crítica desde «ExploracionPie» hasta «ConsultaMedicaResultados» cuando transcurren más de 30 días
* **R08** [time] Define una sección crítica desde «CribadoRenal» hasta «IntensificacionHipolipemiantes» cuando transcurren más de 30 días
* **R09** [mixed] Define una región desde «CitaDemorada» hasta «EducacionDiabetologica» en los casos con AccesoDigital igual a «si»
* **R10** [mixed] Define una sección crítica desde «RevisionAnualCronicos» hasta «ObjetivoTAAlcanzado» solo en los casos con Adherencia igual a «buena»

### Milestones

* **M01** [act] Define «SolicitudCitaApp» como hito
* **M02** [act] Marca «EvaluacionAnualIndicadores» y «EducacionDiabetologica» como hitos
* **M03** [act] Usa «AtencionUrgenciasNoUrgente» como hito
* **M04** [act] Define «AltaUrgenciasNoUrgente» como hito
* **M05** [act] Define «ComplementaDocumentacion» y «VisitaPostAlta» como hitos
* **M06** [act] Marca «CitaPostAltaOrdinaria», «AtencionUrgenteCentroSalud» y «AnaliticaRepetida» como hitos
* **M07** [act] Define «IntensificacionAntihipertensivos» como hito
* **M08** [act] Marca «ExacerbacionEPOC» y «IntensificacionHipolipemiantes» como hitos
* **M09** [act] Define «IntensificacionAntidiabeticos», «IntensificacionHipolipemiantes» y «DescompensacionGlucemica» como hitos
* **M10** [act] Define «Retinografia» como hito


## bpic2012

### Filters

* **F01** [act] Limit the cases to: cases in which 'W_Nabellen incomplete dossiers' occurs
* **F02** [act] Limit the cases to: cases that went through 'W_Afhandelen leads'
* **F33** [var] Limit the cases to: cases with AMOUNT_REQ greater than 6500
* **F34** [var] Limit the cases to: cases with AMOUNT_REQ between 6500 and 15000
* **F36** [time] Limit the cases to: cases with 'O_SELECTED' and, later, 'W_Completeren aanvraag' in less than 1 minute
* **F37** [time] Limit the cases to: cases with 'O_SELECTED' and, later, 'W_Nabellen incomplete dossiers' more than 10 days apart
* **F35** [mixed] Limit the cases to: cases with 'W_Nabellen incomplete dossiers' and AMOUNT_REQ greater than 10000
* **F39** [mixed] Limit the cases to: cases with AMOUNT_REQ greater than 7500 and 'O_CANCELLED' less than 7 days after 'O_SELECTED'

### Circuits

* **C01** [act] Create circuits for this: separate the applications by how they end: declined, validated, lead handling or application completion
* **C02** [act] Create circuits for this: classify the applications by whether they had an incomplete-file callback, lead handling or a declined offer (first match wins)
* **C03** [act] Create circuits for this: separate the applications that went from partly submitted to offer sent, those that were only partly submitted, and the rest
* **C04** [act] Create circuits for this: separate the cases by how they end: 'A_DECLINED' or 'W_Valideren aanvraag'
* **C05** [var] Create circuits for this: split the cases in two groups by AMOUNT_REQ: up to 10000 and more than 10000
* **C06** [time] Create three circuits: 'O_SELECTED' then 'W_Valideren aanvraag' in less than 10 days, the rest of those with 'O_SELECTED' then 'W_Valideren aanvraag', and all the others
* **C07** [time] Make three circuits by the time between 'A_PREACCEPTED' and 'W_Nabellen incomplete dossiers': less than 10 days, longer, and those without that stretch
* **C08** [time] Create three circuits: 'O_CREATED' then 'O_SENT_BACK' in less than 7 days, the rest of those with 'O_CREATED' then 'O_SENT_BACK', and all the others
* **C09** [time] Make three circuits by the time between 'A_FINALIZED' and 'O_SENT_BACK': less than 7 days, longer, and those without that stretch
* **C10** [time] Create three circuits: 'A_FINALIZED' then 'O_ACCEPTED' in less than 10 days, the rest of those with 'A_FINALIZED' then 'O_ACCEPTED', and all the others
* **C11** [time] Make three circuits by the time between 'O_SENT' and 'O_SENT_BACK': less than 7 days, longer, and those without that stretch
* **C12** [act] Create circuits for this: classify the cases by their last step: 'A_DECLINED', 'W_Valideren aanvraag' or 'W_Afhandelen leads'

### Regions

* **R01** [act] Define a region from 'A_PARTLYSUBMITTED' to 'O_SENT'
* **R02** [act] Define a critical section from 'A_PREACCEPTED' to 'O_CREATED'
* **R03** [act] Define a region from 'A_ACCEPTED' to 'O_ACCEPTED'
* **R04** [act] Define two critical sections: from 'A_ACCEPTED' to 'A_DECLINED' and from 'A_APPROVED' to 'A_REGISTERED'
* **R05** [act] Define two regions: from 'A_ACCEPTED' to 'O_ACCEPTED' and from 'A_DECLINED' to 'W_Afhandelen leads'
* **R06** [time] Define a region from 'O_SELECTED' to 'W_Valideren aanvraag' when less than 10 days pass between them
* **R07** [time] Define a critical section from 'A_PREACCEPTED' to 'W_Nabellen incomplete dossiers' when more than 14 days elapse
* **R08** [time] Define a region from 'O_CREATED' to 'O_SENT_BACK' when less than 7 days pass between them
* **R09** [time] Define a critical section from 'A_FINALIZED' to 'O_SENT_BACK' when more than 10 days elapse
* **R10** [time] Define a region from 'A_FINALIZED' to 'O_ACCEPTED' when less than 10 days pass between them

### Milestones

* **M01** [act] Define 'A_CANCELLED' as a milestone
* **M02** [act] Mark 'O_SELECTED' and 'O_SENT_BACK' as milestones
* **M03** [act] Use 'O_ACCEPTED' as a milestone
* **M04** [act] Define 'A_APPROVED' as a milestone
* **M05** [act] Define 'A_ACTIVATED' and 'W_Completeren aanvraag' as milestones
* **M06** [act] Mark 'A_CANCELLED', 'W_Afhandelen leads' and 'A_PREACCEPTED' as milestones
* **M07** [act] Define 'W_Nabellen offertes' as a milestone
* **M08** [act] Mark 'W_Afhandelen leads' and 'A_FINALIZED' as milestones
* **M09** [act] Define 'W_Valideren aanvraag', 'A_PREACCEPTED' and 'O_SENT' as milestones
* **M10** [act] Define 'O_CREATED' as a milestone


## bpic2017

### Filters

* **F01** [act] Limit the cases to: cases in which 'A_Pending' occurs
* **F02** [act] Limit the cases to: cases that went through 'W_Complete application'
* **F33** [var] Limit the cases to: cases with RequestedAmount greater than 8600.0
* **F34** [var] Limit the cases to: cases with RequestedAmount between 8600.0 and 20000.0
* **F39** [time] Limit the cases to: cases with 'O_Returned' and, later, 'A_Pending' in less than 4 days
* **F40** [time] Limit the cases to: cases with 'O_Returned' and, later, 'O_Accepted' more than 5 days apart
* **F38** [mixed] Limit the cases to: cases with 'A_Pending' and RequestedAmount greater than 12500.0
* **F42** [mixed] Limit the cases to: cases with RequestedAmount greater than 10000.0 and 'A_Incomplete' less than 10 days after 'O_Sent (mail and online)'

### Circuits

* **C01** [act] Create circuits for this: separate the applications by how they end: offer cancelled, pending, validation work or offer refused
* **C02** [act] Create circuits for this: classify the applications by whether they became pending, had completion work or had validation work (first match wins)
* **C03** [act] Create circuits for this: separate the applications where an offer was created and then denied, those with only an offer created, and the rest
* **C04** [act] Create circuits for this: separate the cases by how they end: 'O_Cancelled' or 'A_Pending'
* **C05** [var] Create circuits for this: separate the cases by ApplicationType: 'New credit' or 'Limit raise'
* **C06** [var] Create circuits for this: group the cases by LoanGoal: 'Car' or 'Home improvement'
* **C07** [var] Create circuits for this: split the cases in two groups by RequestedAmount: up to 12500.0 and more than 12500.0
* **C08** [time] Create three circuits: 'O_Returned' then 'A_Pending' in less than 4 days, the rest of those with 'O_Returned' then 'A_Pending', and all the others
* **C09** [time] Make three circuits by the time between 'O_Create Offer' and 'A_Cancelled': less than 30 days, longer, and those without that stretch
* **C10** [mixed] Create three circuits: cases with ApplicationType equal to 'New credit' that go through 'A_Pending', the rest of the cases with ApplicationType equal to 'New credit', and all the others
* **C11** [mixed] Make three circuits: LoanGoal equal to 'Car' and 'A_Cancelled', only LoanGoal equal to 'Car', and the rest
* **C12** [time] Create three circuits: 'A_Submitted' then 'W_Handle leads' in less than 1 hour, the rest of those with 'A_Submitted' then 'W_Handle leads', and all the others

### Regions

* **R01** [act] Define a region from 'O_Create Offer' to 'A_Denied'
* **R02** [act] Define a critical section from 'W_Complete application' to 'O_Sent (mail and online)'
* **R03** [act] Define a region from 'A_Incomplete' to 'A_Pending'
* **R04** [act] Define two critical sections: from 'W_Handle leads' to 'A_Accepted' and from 'A_Concept' to 'A_Cancelled'
* **R05** [act] Define two regions: from 'A_Incomplete' to 'A_Pending' and from 'O_Sent (mail and online)' to 'A_Incomplete'
* **R06** [time] Define a region from 'O_Returned' to 'A_Pending' when less than 4 days pass between them
* **R07** [time] Define a critical section from 'O_Create Offer' to 'A_Cancelled' when more than 30 days elapse
* **R08** [time] Define a region from 'A_Submitted' to 'W_Handle leads' when less than 1 hour pass between them
* **R09** [mixed] Define a region from 'A_Incomplete' to 'A_Pending' in the cases with ApplicationType equal to 'New credit'
* **R10** [mixed] Define a critical section from 'O_Sent (mail and online)' to 'A_Incomplete' only in the cases with LoanGoal equal to 'Car'

### Milestones

* **M01** [act] Define 'A_Incomplete' as a milestone
* **M02** [act] Mark 'A_Pending' and 'W_Validate application' as milestones
* **M03** [act] Use 'A_Cancelled' as a milestone
* **M04** [act] Define 'A_Validating' as a milestone
* **M05** [act] Define 'A_Incomplete' and 'O_Accepted' as milestones
* **M06** [act] Mark 'A_Submitted', 'A_Denied' and 'W_Call incomplete files' as milestones
* **M07** [act] Define 'O_Refused' as a milestone
* **M08** [act] Mark 'O_Refused' and 'W_Validate application' as milestones
* **M09** [act] Define 'W_Complete application', 'O_Accepted' and 'O_Refused' as milestones
* **M10** [act] Define 'O_Returned' as a milestone


## bpic2019

### Filters

* **F01** [act] Limit the cases to: cases in which 'Remove Payment Block' occurs
* **F02** [act] Limit the cases to: cases that went through 'Create Purchase Requisition Item'
* **F34** [var] Limit the cases to: cases with GR-Based Inv. Verif. equal to 'true'
* **F35** [var] Limit the cases to: cases with Spend classification text equal to 'NPR'
* **F39** [time] Limit the cases to: cases with 'Vendor creates invoice' and, later, 'Remove Payment Block' in less than 20 days
* **F40** [time] Limit the cases to: cases with 'Receive Order Confirmation' and, later, 'Vendor creates invoice' more than 7 days apart
* **F38** [mixed] Limit the cases to: cases with 'Receive Order Confirmation' and, later, 'Record Invoice Receipt', where Spend area text is 'Packaging'
* **F36** [var] Limit the cases to: cases with Item Category equal to 'Consignment' and Item Type equal to 'Consignment'

### Circuits

* **C01** [act] Create circuits for this: separate the items by how they end: invoice cleared, invoice recorded, goods received or order item deleted
* **C02** [act] Create circuits for this: classify the items by whether they had a payment block removal, started from a requisition or had a quantity change (first match wins)
* **C03** [act] Create circuits for this: separate the items with an order confirmation followed by an invoice receipt, those with only an order confirmation, and the rest
* **C04** [act] Create circuits for this: separate the cases by how they end: 'Clear Invoice' or 'Record Invoice Receipt'
* **C05** [var] Create circuits for this: group the cases by GR-Based Inv. Verif.: 'false' or 'true'
* **C06** [time] Create three circuits: 'Receive Order Confirmation' then 'Clear Invoice' in less than 30 days, the rest of those with 'Receive Order Confirmation' then 'Clear Invoice', and all the others
* **C07** [time] Make three circuits by the time between 'Create Purchase Requisition Item' and 'Clear Invoice': less than 30 days, longer, and those without that stretch
* **C08** [mixed] Create three circuits: cases with Company equal to 'companyID_0000' that go through 'Clear Invoice', the rest of the cases with Company equal to 'companyID_0000', and all the others
* **C09** [mixed] Make three circuits: Document Type equal to 'Standard PO' and 'Change Quantity', only Document Type equal to 'Standard PO', and the rest
* **C10** [time] Create three circuits: 'Record Goods Receipt' then 'Remove Payment Block' in less than 20 days, the rest of those with 'Record Goods Receipt' then 'Remove Payment Block', and all the others
* **C11** [time] Make three circuits by the time between 'Change Quantity' and 'Record Goods Receipt': less than 3 days, longer, and those without that stretch
* **C12** [time] Create three circuits: 'Record Goods Receipt' then 'Clear Invoice' in less than 30 days, the rest of those with 'Record Goods Receipt' then 'Clear Invoice', and all the others

### Regions

* **R01** [act] Define a region from 'Receive Order Confirmation' to 'Record Invoice Receipt'
* **R02** [act] Define a critical section from 'Remove Payment Block' to 'Clear Invoice'
* **R03** [act] Define a region from 'Create Purchase Requisition Item' to 'Remove Payment Block'
* **R04** [act] Define two critical sections: from 'Vendor creates invoice' to 'Record Goods Receipt' and from 'Create Purchase Order Item' to 'Change Quantity'
* **R05** [act] Define two regions: from 'Create Purchase Requisition Item' to 'Remove Payment Block' and from 'Record Invoice Receipt' to 'Remove Payment Block'
* **R06** [time] Define a region from 'Receive Order Confirmation' to 'Clear Invoice' when less than 30 days pass between them
* **R07** [time] Define a critical section from 'Create Purchase Requisition Item' to 'Clear Invoice' when more than 30 days elapse
* **R08** [time] Define a region from 'Record Goods Receipt' to 'Remove Payment Block' when less than 20 days pass between them
* **R09** [mixed] Define a region from 'Create Purchase Requisition Item' to 'Remove Payment Block' in the cases with Company equal to 'companyID_0000'
* **R10** [mixed] Define a critical section from 'Record Invoice Receipt' to 'Remove Payment Block' only in the cases with Document Type equal to 'Standard PO'

### Milestones

* **M01** [act] Define 'Create Purchase Requisition Item' as a milestone
* **M02** [act] Mark 'Clear Invoice' and 'Vendor creates debit memo' as milestones
* **M03** [act] Use 'Cancel Invoice Receipt' as a milestone
* **M04** [act] Define 'Delete Purchase Order Item' as a milestone
* **M05** [act] Define 'Record Invoice Receipt' and 'Vendor creates debit memo' as milestones
* **M06** [act] Mark 'Record Service Entry Sheet', 'Receive Order Confirmation' and 'Create Purchase Requisition Item' as milestones
* **M07** [act] Define 'Change Price' as a milestone
* **M08** [act] Mark 'Clear Invoice' and 'Record Invoice Receipt' as milestones
* **M09** [act] Define 'Create Purchase Requisition Item', 'Clear Invoice' and 'Record Invoice Receipt' as milestones
* **M10** [act] Define 'Change Quantity' as a milestone


## bpic2020_domestic_declarations

### Filters

* **F01** [act] Limit the cases to: cases in which 'Declaration REJECTED by EMPLOYEE' occurs
* **F02** [act] Limit the cases to: cases that went through 'Declaration APPROVED by PRE_APPROVER'
* **F29** [var] Limit the cases to: cases with Amount greater than 29.2
* **F30** [var] Limit the cases to: cases with Amount between 29.2 and 71.5
* **F32** [time] Limit the cases to: cases with 'Declaration APPROVED by BUDGET OWNER' and, later, 'Payment Handled' in less than 7 days
* **F33** [time] Limit the cases to: cases with 'Declaration APPROVED by BUDGET OWNER' and, later, 'Declaration FINAL_APPROVED by SUPERVISOR' more than 2 days apart
* **F31** [mixed] Limit the cases to: cases with 'Declaration REJECTED by EMPLOYEE' and Amount greater than 41.5
* **F35** [mixed] Limit the cases to: cases with Amount greater than 32.7 and 'Request Payment' less than 6 days after 'Declaration APPROVED by BUDGET OWNER'

### Circuits

* **C01** [act] Create circuits for this: separate the declarations by how they end: payment handled, rejected by the employee, saved by the employee or rejected by an unknown role
* **C02** [act] Create circuits for this: classify the declarations by whether the employee rejected, the pre-approver approved or the budget owner approved (first match wins)
* **C03** [act] Create circuits for this: separate the declarations rejected by the administration and later approved by it, those only rejected by the administration, and the rest
* **C04** [act] Create circuits for this: classify the cases by whether they go through 'Declaration APPROVED by BUDGET OWNER', 'Declaration APPROVED by PRE_APPROVER' or 'Declaration REJECTED by ADMINISTRATION' (first match wins)
* **C05** [var] Create circuits for this: separate the cases by Amount: up to 41.5 and above 41.5
* **C06** [time] Create three circuits: 'Declaration REJECTED by EMPLOYEE' then 'Request Payment' in less than 5 days, the rest of those with 'Declaration REJECTED by EMPLOYEE' then 'Request Payment', and all the others
* **C07** [time] Make three circuits by the time between 'Declaration SUBMITTED by EMPLOYEE' and 'Declaration REJECTED by EMPLOYEE': less than 1 day, longer, and those without that stretch
* **C08** [time] Create three circuits: 'Declaration REJECTED by EMPLOYEE' then 'Declaration FINAL_APPROVED by SUPERVISOR' in less than 2 days, the rest of those with 'Declaration REJECTED by EMPLOYEE' then 'Declaration FINAL_APPROVED by SUPERVISOR', and all the others
* **C09** [time] Make three circuits by the time between 'Declaration REJECTED by EMPLOYEE' and 'Declaration SUBMITTED by EMPLOYEE': less than 3 hours, longer, and those without that stretch
* **C10** [time] Create three circuits: 'Declaration REJECTED by EMPLOYEE' then 'Payment Handled' in less than 7 days, the rest of those with 'Declaration REJECTED by EMPLOYEE' then 'Payment Handled', and all the others
* **C11** [time] Make three circuits by the time between 'Declaration REJECTED by ADMINISTRATION' and 'Request Payment': less than 7 days, longer, and those without that stretch
* **C12** [act] Create circuits for this: separate the cases by whether they went through 'Declaration REJECTED by ADMINISTRATION', 'Declaration APPROVED by ADMINISTRATION' or 'Declaration REJECTED by EMPLOYEE', in this order of priority

### Regions

* **R01** [act] Define a region from 'Declaration REJECTED by ADMINISTRATION' to 'Declaration APPROVED by ADMINISTRATION'
* **R02** [act] Define a critical section from 'Declaration REJECTED by EMPLOYEE' to 'Request Payment'
* **R03** [act] Define a region from 'Declaration APPROVED by ADMINISTRATION' to 'Request Payment'
* **R04** [act] Define two critical sections: from 'Declaration SUBMITTED by EMPLOYEE' to 'Declaration APPROVED by PRE_APPROVER' and from 'Declaration APPROVED by BUDGET OWNER' to 'Payment Handled'
* **R05** [act] Define two regions: from 'Declaration REJECTED by EMPLOYEE' to 'Request Payment' and from 'Declaration APPROVED by ADMINISTRATION' to 'Request Payment'
* **R06** [time] Define a region from 'Declaration REJECTED by EMPLOYEE' to 'Request Payment' when less than 5 days pass between them
* **R07** [time] Define a critical section from 'Declaration SUBMITTED by EMPLOYEE' to 'Declaration REJECTED by EMPLOYEE' when more than 2 days elapse
* **R08** [time] Define a region from 'Declaration REJECTED by EMPLOYEE' to 'Declaration FINAL_APPROVED by SUPERVISOR' when less than 2 days pass between them
* **R09** [time] Define a critical section from 'Declaration REJECTED by EMPLOYEE' to 'Declaration SUBMITTED by EMPLOYEE' when more than 14 hours elapse
* **R10** [time] Define a region from 'Declaration REJECTED by EMPLOYEE' to 'Payment Handled' when less than 7 days pass between them

### Milestones

* **M01** [act] Define 'Declaration REJECTED by ADMINISTRATION' as a milestone
* **M02** [act] Mark 'Declaration APPROVED by ADMINISTRATION' and 'Declaration REJECTED by EMPLOYEE' as milestones
* **M03** [act] Use 'Declaration REJECTED by SUPERVISOR' as a milestone
* **M04** [act] Define 'Declaration APPROVED by BUDGET OWNER' as a milestone
* **M05** [act] Define 'Declaration REJECTED by EMPLOYEE' and 'Declaration APPROVED by PRE_APPROVER' as milestones
* **M06** [act] Mark 'Declaration APPROVED by PRE_APPROVER', 'Declaration APPROVED by ADMINISTRATION' and 'Declaration REJECTED by ADMINISTRATION' as milestones
* **M07** [act] Define 'Declaration APPROVED by PRE_APPROVER' and 'Declaration REJECTED by EMPLOYEE' as milestones
* **M08** [act] Mark 'Declaration REJECTED by EMPLOYEE', 'Declaration REJECTED by ADMINISTRATION' and 'Declaration APPROVED by PRE_APPROVER' as milestones
* **M09** [act] Define 'Declaration REJECTED by ADMINISTRATION' and 'Declaration APPROVED by ADMINISTRATION' as milestones
* **M10** [act] Mark 'Declaration APPROVED by BUDGET OWNER', 'Declaration REJECTED by ADMINISTRATION' and 'Declaration REJECTED by SUPERVISOR' as milestones


## bpic2020_international_declarations

### Filters

* **F01** [act] Limit the cases to: cases in which 'Declaration APPROVED by PRE_APPROVER' occurs
* **F02** [act] Limit the cases to: cases that went through 'Permit APPROVED by SUPERVISOR'
* **F33** [var] Limit the cases to: cases with Permit RequestedBudget between 658.5 and 1429.8
* **F34** [var] Limit the cases to: cases with Permit TaskNumber equal to 'task 427'
* **F35** [time] Limit the cases to: cases with 'Permit APPROVED by BUDGET OWNER' and, later, 'Permit FINAL_APPROVED by SUPERVISOR' in less than 2 days
* **F36** [time] Limit the cases to: cases with 'Permit SUBMITTED by EMPLOYEE' and, later, 'Declaration APPROVED by BUDGET OWNER' more than 30 days apart
* **F38** [mixed] Limit the cases to: cases with Permit RequestedBudget greater than 765.1 and 'Request Payment' less than 7 days after 'Declaration REJECTED by EMPLOYEE'
* **F37** [time] Limit the cases to: cases with 'Declaration REJECTED by ADMINISTRATION' and, later, 'Request Payment' between 6 days and 20 days after

### Circuits

* **C01** [act] Create circuits for this: separate the trips by how they end: payment handled, end of the trip, rejected by the employee or saved by the employee
* **C02** [act] Create circuits for this: classify the trips by whether the pre-approver approved the declaration, the supervisor approved the permit or the director gave final approval (first match wins)
* **C03** [act] Create circuits for this: separate the trips with a reminder followed by payment, those with only a reminder, and the rest
* **C04** [act] Create circuits for this: separate the cases by how they end: 'Payment Handled' or 'End trip'
* **C05** [var] Create circuits for this: separate the trips charged to task 427 from those with an unknown task
* **C06** [var] Create circuits for this: separate the cases by AdjustedAmount: up to 509.1 and above 509.1
* **C07** [var] Create circuits for this: split the cases in two groups by Amount: up to 509.1 and more than 509.1
* **C08** [var] Create circuits for this: separate the cases by OriginalAmount: up to 509.1 and above 509.1
* **C09** [time] Create three circuits: 'Permit APPROVED by BUDGET OWNER' then 'Permit FINAL_APPROVED by SUPERVISOR' in less than 2 days, the rest of those with 'Permit APPROVED by BUDGET OWNER' then 'Permit FINAL_APPROVED by SUPERVISOR', and all the others
* **C10** [time] Make three circuits by the time between 'Declaration APPROVED by ADMINISTRATION' and 'Declaration FINAL_APPROVED by SUPERVISOR': less than 2 days, longer, and those without that stretch
* **C11** [mixed] Create three circuits: cases with Permit TaskNumber equal to 'task 427' that go through 'Permit FINAL_APPROVED by SUPERVISOR', the rest of the cases with Permit TaskNumber equal to 'task 427', and all the others
* **C12** [time] Create three circuits: 'Declaration APPROVED by SUPERVISOR' then 'Request Payment' in less than 3 days, the rest of those with 'Declaration APPROVED by SUPERVISOR' then 'Request Payment', and all the others

### Regions

* **R01** [act] Define a region from 'Send Reminder' to 'Payment Handled'
* **R02** [act] Define a critical section from 'Permit APPROVED by BUDGET OWNER' to 'Declaration APPROVED by BUDGET OWNER'
* **R03** [act] Define a region from 'Permit APPROVED by BUDGET OWNER' to 'Declaration FINAL_APPROVED by SUPERVISOR'
* **R04** [act] Define two critical sections: from 'Permit APPROVED by ADMINISTRATION' to 'Permit APPROVED by SUPERVISOR' and from 'Declaration SUBMITTED by EMPLOYEE' to 'Start trip'
* **R05** [act] Define two regions: from 'Permit APPROVED by BUDGET OWNER' to 'Declaration FINAL_APPROVED by SUPERVISOR' and from 'Declaration REJECTED by EMPLOYEE' to 'Declaration FINAL_APPROVED by SUPERVISOR'
* **R06** [time] Define a region from 'Permit APPROVED by BUDGET OWNER' to 'Permit FINAL_APPROVED by SUPERVISOR' when less than 2 days pass between them
* **R07** [time] Define a critical section from 'Declaration APPROVED by ADMINISTRATION' to 'Declaration FINAL_APPROVED by SUPERVISOR' when more than 3 days elapse
* **R08** [time] Define a region from 'Declaration APPROVED by SUPERVISOR' to 'Request Payment' when less than 3 days pass between them
* **R09** [mixed] Define a region from 'Permit APPROVED by BUDGET OWNER' to 'Declaration FINAL_APPROVED by SUPERVISOR' in the cases with Permit TaskNumber equal to 'task 427'
* **R10** [time] Define a critical section from 'Declaration SUBMITTED by EMPLOYEE' to 'Declaration FINAL_APPROVED by DIRECTOR' when more than 6 days elapse

### Milestones

* **M01** [act] Define 'Permit APPROVED by SUPERVISOR' as a milestone
* **M02** [act] Mark 'Declaration APPROVED by SUPERVISOR' and 'Send Reminder' as milestones
* **M03** [act] Use 'Permit FINAL_APPROVED by SUPERVISOR' as a milestone
* **M04** [act] Define 'Declaration FINAL_APPROVED by DIRECTOR' as a milestone
* **M05** [act] Define 'Permit REJECTED by EMPLOYEE' and 'Send Reminder' as milestones
* **M06** [act] Mark 'Permit APPROVED by ADMINISTRATION', 'Declaration REJECTED by EMPLOYEE' and 'Permit APPROVED by PRE_APPROVER' as milestones
* **M07** [act] Define 'Declaration APPROVED by PRE_APPROVER' as a milestone
* **M08** [act] Mark 'Send Reminder' and 'Permit APPROVED by PRE_APPROVER' as milestones
* **M09** [act] Define 'Declaration FINAL_APPROVED by DIRECTOR', 'Declaration APPROVED by BUDGET OWNER' and 'Permit APPROVED by ADMINISTRATION' as milestones
* **M10** [act] Define 'Permit APPROVED by BUDGET OWNER' as a milestone


## bpic2020_prepaid_travel_costs

### Filters

* **F01** [act] Limit the cases to: cases in which 'Request For Payment REJECTED by ADMINISTRATION' occurs
* **F02** [act] Limit the cases to: cases that went through 'Request For Payment REJECTED by EMPLOYEE'
* **F32** [var] Limit the cases to: cases with Permit RequestedBudget between 1031.8 and 2193.9
* **F33** [var] Limit the cases to: cases with Permit OrganizationalEntity equal to 'UNKNOWN'
* **F38** [time] Limit the cases to: cases with 'Permit APPROVED by ADMINISTRATION' and, later, 'Permit FINAL_APPROVED by DIRECTOR' in less than 5 days
* **F39** [time] Limit the cases to: cases with 'Permit APPROVED by ADMINISTRATION' and, later, 'Permit APPROVED by SUPERVISOR' more than 3 days apart
* **F37** [mixed] Limit the cases to: cases with 'Request For Payment APPROVED by PRE_APPROVER' and, later, 'Payment Handled', where Permit TaskNumber is 'task 427'
* **F41** [mixed] Limit the cases to: cases with Permit RequestedBudget greater than 1181.2 and 'Request For Payment SUBMITTED by EMPLOYEE' less than 20 days after 'Permit APPROVED by BUDGET OWNER'

### Circuits

* **C01** [act] Create circuits for this: separate the cases by how they end: payment handled, rejected by the employee, saved by the employee or final supervisor permit approval
* **C02** [act] Create circuits for this: classify the cases by whether the administration rejected the request, the employee rejected it or the supervisor approved the permit (first match wins)
* **C03** [act] Create circuits for this: separate the cases where the pre-approver approved and payment followed, those with only the pre-approver's approval, and the rest
* **C04** [act] Create circuits for this: separate the cases by how they end: 'Payment Handled' or 'Request For Payment REJECTED by EMPLOYEE'
* **C05** [var] Create circuits for this: separate the cases by organisational unit: 65455, 65456, 65458 and 65454
* **C06** [var] Create circuits for this: separate the cases by Activity: 'activity 505' or 'UNKNOWN'
* **C07** [var] Create circuits for this: group the cases by Permit OrganizationalEntity: 'organizational unit 65455' or 'organizational unit 65456'
* **C08** [var] Create one circuit for each value of Permit TaskNumber: 'task 427' or 'UNKNOWN'
* **C09** [time] Create three circuits: 'Request For Payment REJECTED by ADMINISTRATION' then 'Request For Payment FINAL_APPROVED by SUPERVISOR' in less than 7 days, the rest of those with 'Request For Payment REJECTED by ADMINISTRATION' then 'Request For Payment FINAL_APPROVED by SUPERVISOR', and all the others
* **C10** [time] Make three circuits by the time between 'Permit APPROVED by PRE_APPROVER' and 'Request For Payment SUBMITTED by EMPLOYEE': less than 10 days, longer, and those without that stretch
* **C11** [mixed] Create three circuits: cases with Activity equal to 'activity 505' that go through 'Permit FINAL_APPROVED by SUPERVISOR', the rest of the cases with Activity equal to 'activity 505', and all the others
* **C12** [mixed] Make three circuits: Permit OrganizationalEntity equal to 'organizational unit 65455' and 'Request For Payment REJECTED by ADMINISTRATION', only Permit OrganizationalEntity equal to 'organizational unit 65455', and the rest

### Regions

* **R01** [act] Define a region from 'Request For Payment APPROVED by PRE_APPROVER' to 'Payment Handled'
* **R02** [act] Define a critical section from 'Permit SUBMITTED by EMPLOYEE' to 'Request For Payment REJECTED by EMPLOYEE'
* **R03** [act] Define a region from 'Request For Payment APPROVED by ADMINISTRATION' to 'Request For Payment APPROVED by BUDGET OWNER'
* **R04** [act] Define two critical sections: from 'Permit FINAL_APPROVED by DIRECTOR' to 'Request For Payment SUBMITTED by EMPLOYEE' and from 'Permit APPROVED by SUPERVISOR' to 'Request For Payment APPROVED by BUDGET OWNER'
* **R05** [act] Define two regions: from 'Request For Payment APPROVED by ADMINISTRATION' to 'Request For Payment APPROVED by BUDGET OWNER' and from 'Request For Payment REJECTED by EMPLOYEE' to 'Payment Handled'
* **R06** [time] Define a region from 'Request For Payment REJECTED by ADMINISTRATION' to 'Request For Payment FINAL_APPROVED by SUPERVISOR' when less than 7 days pass between them
* **R07** [time] Define a critical section from 'Permit APPROVED by PRE_APPROVER' to 'Request For Payment SUBMITTED by EMPLOYEE' when more than 20 days elapse
* **R08** [time] Define a region from 'Permit FINAL_APPROVED by DIRECTOR' to 'Request For Payment FINAL_APPROVED by SUPERVISOR' when less than 20 days pass between them
* **R09** [mixed] Define a region from 'Request For Payment APPROVED by ADMINISTRATION' to 'Request For Payment APPROVED by BUDGET OWNER' in the cases with Activity equal to 'activity 505'
* **R10** [mixed] Define a critical section from 'Request For Payment REJECTED by EMPLOYEE' to 'Payment Handled' only in the cases with Permit OrganizationalEntity equal to 'organizational unit 65455'

### Milestones

* **M01** [act] Define 'Permit REJECTED by EMPLOYEE' as a milestone
* **M02** [act] Mark 'Request For Payment REJECTED by EMPLOYEE' and 'Permit APPROVED by SUPERVISOR' as milestones
* **M03** [act] Use 'Request For Payment APPROVED by ADMINISTRATION' as a milestone
* **M04** [act] Define 'Request For Payment APPROVED by PRE_APPROVER' as a milestone
* **M05** [act] Define 'Permit APPROVED by SUPERVISOR' and 'Request For Payment APPROVED by PRE_APPROVER' as milestones
* **M06** [act] Mark 'Request For Payment APPROVED by PRE_APPROVER', 'Request For Payment APPROVED by BUDGET OWNER' and 'Permit FINAL_APPROVED by SUPERVISOR' as milestones
* **M07** [act] Define 'Permit APPROVED by BUDGET OWNER' as a milestone
* **M08** [act] Mark 'Permit FINAL_APPROVED by DIRECTOR' and 'Permit REJECTED by EMPLOYEE' as milestones
* **M09** [act] Define 'Permit APPROVED by PRE_APPROVER', 'Permit APPROVED by SUPERVISOR' and 'Permit FINAL_APPROVED by DIRECTOR' as milestones
* **M10** [act] Define 'Permit SUBMITTED by EMPLOYEE' as a milestone


## bpic2020_request_for_payment

### Filters

* **F01** [act] Limit the cases to: cases in which 'Request For Payment REJECTED by EMPLOYEE' occurs
* **F02** [act] Limit the cases to: cases that went through 'Request For Payment APPROVED by PRE_APPROVER'
* **F32** [var] Limit the cases to: cases with RequestedAmount between 53.5 and 200.7
* **F33** [var] Limit the cases to: cases with Activity equal to 'activity 505'
* **F35** [time] Limit the cases to: cases with 'Request For Payment APPROVED by BUDGET OWNER' and, later, 'Payment Handled' in less than 7 days
* **F36** [time] Limit the cases to: cases with 'Request For Payment APPROVED by ADMINISTRATION' and, later, 'Request For Payment APPROVED by BUDGET OWNER' more than 1 day apart
* **F34** [mixed] Limit the cases to: cases with 'Request For Payment REJECTED by EMPLOYEE' and RequestedAmount greater than 94.3
* **F38** [mixed] Limit the cases to: cases with RequestedAmount greater than 64.4 and 'Request For Payment FINAL_APPROVED by SUPERVISOR' less than 3 days after 'Request For Payment APPROVED by BUDGET OWNER'

### Circuits

* **C01** [act] Create circuits for this: separate the requests by how they end: payment handled, rejected by the employee, saved by the employee or rejected by an unknown role
* **C02** [act] Create circuits for this: classify the requests by whether the employee rejected, the pre-approver approved or the budget owner approved (first match wins)
* **C03** [act] Create circuits for this: separate the requests rejected by the administration and later approved by it, those only rejected by the administration, and the rest
* **C04** [act] Create circuits for this: separate the cases by how they end: 'Payment Handled' or 'Request For Payment REJECTED by EMPLOYEE'
* **C05** [var] Create one circuit for each value of Activity: 'UNKNOWN' or 'activity 505'
* **C06** [var] Create circuits for this: separate the cases by RequestedAmount: up to 94.3 and above 94.3
* **C07** [time] Create three circuits: 'Request For Payment REJECTED by EMPLOYEE' then 'Request For Payment FINAL_APPROVED by SUPERVISOR' in less than 5 days, the rest of those with 'Request For Payment REJECTED by EMPLOYEE' then 'Request For Payment FINAL_APPROVED by SUPERVISOR', and all the others
* **C08** [time] Make three circuits by the time between 'Request For Payment SUBMITTED by EMPLOYEE' and 'Request For Payment REJECTED by EMPLOYEE': less than 1 day, longer, and those without that stretch
* **C09** [mixed] Create three circuits: cases with Activity equal to 'UNKNOWN' that go through 'Request For Payment APPROVED by BUDGET OWNER', the rest of the cases with Activity equal to 'UNKNOWN', and all the others
* **C10** [time] Create three circuits: 'Request For Payment REJECTED by ADMINISTRATION' then 'Request For Payment REJECTED by EMPLOYEE' in less than 1 day, the rest of those with 'Request For Payment REJECTED by ADMINISTRATION' then 'Request For Payment REJECTED by EMPLOYEE', and all the others
* **C11** [time] Make three circuits by the time between 'Request For Payment SUBMITTED by EMPLOYEE' and 'Request For Payment REJECTED by ADMINISTRATION': less than 1 minute, longer, and those without that stretch
* **C12** [time] Create three circuits: 'Request For Payment REJECTED by EMPLOYEE' then 'Request For Payment SUBMITTED by EMPLOYEE' in less than 10 hours, the rest of those with 'Request For Payment REJECTED by EMPLOYEE' then 'Request For Payment SUBMITTED by EMPLOYEE', and all the others

### Regions

* **R01** [act] Define a region from 'Request For Payment REJECTED by ADMINISTRATION' to 'Request For Payment APPROVED by ADMINISTRATION'
* **R02** [act] Define a critical section from 'Request For Payment APPROVED by BUDGET OWNER' to 'Payment Handled'
* **R03** [act] Define a region from 'Request For Payment REJECTED by ADMINISTRATION' to 'Request For Payment SUBMITTED by EMPLOYEE'
* **R04** [act] Define two critical sections: from 'Request For Payment REJECTED by EMPLOYEE' to 'Request For Payment FINAL_APPROVED by SUPERVISOR' and from 'Request For Payment SUBMITTED by EMPLOYEE' to 'Request For Payment APPROVED by PRE_APPROVER'
* **R05** [act] Define two regions: from 'Request For Payment REJECTED by ADMINISTRATION' to 'Request For Payment SUBMITTED by EMPLOYEE' and from 'Request For Payment SUBMITTED by EMPLOYEE' to 'Request For Payment APPROVED by BUDGET OWNER'
* **R06** [time] Define a region from 'Request For Payment REJECTED by EMPLOYEE' to 'Request For Payment FINAL_APPROVED by SUPERVISOR' when less than 5 days pass between them
* **R07** [time] Define a critical section from 'Request For Payment SUBMITTED by EMPLOYEE' to 'Request For Payment REJECTED by EMPLOYEE' when more than 2 days elapse
* **R08** [time] Define a region from 'Request For Payment REJECTED by ADMINISTRATION' to 'Request For Payment REJECTED by EMPLOYEE' when less than 1 day pass between them
* **R09** [mixed] Define a region from 'Request For Payment REJECTED by ADMINISTRATION' to 'Request For Payment SUBMITTED by EMPLOYEE' in the cases with Activity equal to 'UNKNOWN'
* **R10** [time] Define a critical section from 'Request For Payment SUBMITTED by EMPLOYEE' to 'Request For Payment REJECTED by ADMINISTRATION' when more than 1 minute elapse

### Milestones

* **M01** [act] Define 'Request For Payment REJECTED by SUPERVISOR' as a milestone
* **M02** [act] Mark 'Request For Payment APPROVED by ADMINISTRATION' and 'Request For Payment APPROVED by PRE_APPROVER' as milestones
* **M03** [act] Use 'Request For Payment REJECTED by ADMINISTRATION' as a milestone
* **M04** [act] Define 'Request For Payment APPROVED by BUDGET OWNER' as a milestone
* **M05** [act] Define 'Request For Payment REJECTED by ADMINISTRATION' and 'Request For Payment APPROVED by ADMINISTRATION' as milestones
* **M06** [act] Mark 'Request For Payment APPROVED by BUDGET OWNER', 'Request For Payment REJECTED by EMPLOYEE' and 'Request For Payment APPROVED by PRE_APPROVER' as milestones
* **M07** [act] Define 'Request For Payment APPROVED by ADMINISTRATION' and 'Request For Payment APPROVED by BUDGET OWNER' as milestones
* **M08** [act] Mark 'Request For Payment APPROVED by ADMINISTRATION', 'Request For Payment REJECTED by EMPLOYEE' and 'Request For Payment REJECTED by SUPERVISOR' as milestones
* **M09** [act] Define 'Request For Payment APPROVED by ADMINISTRATION' and 'Request For Payment REJECTED by SUPERVISOR' as milestones
* **M10** [act] Mark 'Request For Payment REJECTED by EMPLOYEE', 'Request For Payment REJECTED by SUPERVISOR' and 'Request For Payment REJECTED by ADMINISTRATION' as milestones


## bpic2020_travel_permit

### Filters

* **F01** [act] Limit the cases to: cases in which 'Declaration REJECTED by ADMINISTRATION' occurs
* **F02** [act] Limit the cases to: cases that went through 'Request For Payment FINAL_APPROVED by SUPERVISOR'
* **F34** [var] Limit the cases to: cases with RequestedBudget between 533.7 and 1421.9
* **F35** [var] Limit the cases to: cases with Overspent equal to 'true'
* **F40** [time] Limit the cases to: cases with 'Permit APPROVED by BUDGET OWNER' and, later, 'Declaration SUBMITTED by EMPLOYEE' in less than 30 days
* **F41** [time] Limit the cases to: cases with 'Permit FINAL_APPROVED by SUPERVISOR' and, later, 'Send Reminder' more than 30 days apart
* **F38** [mixed] Limit the cases to: cases with 'Declaration REJECTED by ADMINISTRATION' and RequestedBudget greater than 823.4
* **F39** [mixed] Limit the cases to: cases with 'Request For Payment FINAL_APPROVED by SUPERVISOR' and, later, 'Declaration SUBMITTED by EMPLOYEE', where Overspent is 'true'

### Circuits

* **C01** [act] Create circuits for this: separate the trips by how they end: payment handled, a reminder, end of the trip or a permit rejected by an unknown role
* **C02** [act] Create circuits for this: classify the trips by whether the administration rejected the declaration, a request for payment got final supervisor approval or a reminder was sent (first match wins)
* **C03** [act] Create circuits for this: separate the trips where a request got final supervisor approval and a declaration followed, those with only the final approval, and the rest
* **C04** [act] Create circuits for this: separate the cases by how they end: 'Payment Handled' or 'Send Reminder'
* **C05** [var] Create circuits for this: group the cases by Overspent: 'false' or 'true'
* **C06** [var] Create one circuit for each value of TaskNumber: 'task 427' or 'UNKNOWN'
* **C07** [var] Create circuits for this: separate the cases by RequestedBudget: up to 823.4 and above 823.4
* **C08** [var] Create circuits for this: split the cases in two groups by TotalDeclared: up to 533.0 and more than 533.0
* **C09** [time] Create three circuits: 'Permit SUBMITTED by EMPLOYEE' then 'Declaration REJECTED by EMPLOYEE' in less than 30 days, the rest of those with 'Permit SUBMITTED by EMPLOYEE' then 'Declaration REJECTED by EMPLOYEE', and all the others
* **C10** [time] Make three circuits by the time between 'Permit APPROVED by SUPERVISOR' and 'Declaration APPROVED by ADMINISTRATION': less than 30 days, longer, and those without that stretch
* **C11** [mixed] Create three circuits: cases with Overspent equal to 'false' that go through 'Send Reminder', the rest of the cases with Overspent equal to 'false', and all the others
* **C12** [mixed] Make three circuits: TaskNumber equal to 'task 427' and 'Payment Handled', only TaskNumber equal to 'task 427', and the rest

### Regions

* **R01** [act] Define a region from 'Request For Payment FINAL_APPROVED by SUPERVISOR' to 'Declaration SUBMITTED by EMPLOYEE'
* **R02** [act] Define a critical section from 'Declaration APPROVED by ADMINISTRATION' to 'Declaration FINAL_APPROVED by SUPERVISOR'
* **R03** [act] Define a region from 'Permit FINAL_APPROVED by DIRECTOR' to 'Payment Handled'
* **R04** [act] Define two critical sections: from 'Request For Payment APPROVED by ADMINISTRATION' to 'End trip' and from 'Permit APPROVED by ADMINISTRATION' to 'Declaration APPROVED by BUDGET OWNER'
* **R05** [act] Define two regions: from 'Permit FINAL_APPROVED by DIRECTOR' to 'Payment Handled' and from 'Request For Payment FINAL_APPROVED by SUPERVISOR' to 'Declaration APPROVED by BUDGET OWNER'
* **R06** [time] Define a region from 'Permit SUBMITTED by EMPLOYEE' to 'Declaration REJECTED by EMPLOYEE' when less than 30 days pass between them
* **R07** [time] Define a critical section from 'Permit APPROVED by SUPERVISOR' to 'Declaration APPROVED by ADMINISTRATION' when more than 30 days elapse
* **R08** [time] Define a region from 'Declaration APPROVED by BUDGET OWNER' to 'Declaration FINAL_APPROVED by SUPERVISOR' when less than 2 days pass between them
* **R09** [mixed] Define a region from 'Permit FINAL_APPROVED by DIRECTOR' to 'Payment Handled' in the cases with Overspent equal to 'false'
* **R10** [mixed] Define a critical section from 'Request For Payment FINAL_APPROVED by SUPERVISOR' to 'Declaration APPROVED by BUDGET OWNER' only in the cases with TaskNumber equal to 'task 427'

### Milestones

* **M01** [act] Define 'Permit APPROVED by SUPERVISOR' as a milestone
* **M02** [act] Mark 'Request For Payment REJECTED by ADMINISTRATION' and 'Declaration APPROVED by ADMINISTRATION' as milestones
* **M03** [act] Use 'Permit FINAL_APPROVED by SUPERVISOR' as a milestone
* **M04** [act] Define 'Request For Payment SUBMITTED by EMPLOYEE' as a milestone
* **M05** [act] Define 'Permit FINAL_APPROVED by SUPERVISOR' and 'Permit APPROVED by BUDGET OWNER' as milestones
* **M06** [act] Mark 'Declaration FINAL_APPROVED by DIRECTOR', 'Declaration APPROVED by ADMINISTRATION' and 'Request For Payment APPROVED by BUDGET OWNER' as milestones
* **M07** [act] Define 'Permit APPROVED by ADMINISTRATION' as a milestone
* **M08** [act] Mark 'Permit APPROVED by BUDGET OWNER' and 'Declaration FINAL_APPROVED by SUPERVISOR' as milestones
* **M09** [act] Define 'Declaration REJECTED by ADMINISTRATION', 'Request For Payment REJECTED by EMPLOYEE' and 'Declaration APPROVED by PRE_APPROVER' as milestones
* **M10** [act] Define 'Request For Payment APPROVED by ADMINISTRATION' as a milestone


## dolor_toracico_urgencias

### Filters

* **F01** [act] Limita los casos a: casos en los que ocurre «PlantaCardiologia»
* **F02** [act] Limita los casos a: pacientes/casos que pasaron por «UnidadCoronaria»
* **F32** [var] Limita los casos a: casos con Edad mayor que 57
* **F33** [var] Limita los casos a: casos con Edad entre 57 y 69
* **F40** [time] Limita los casos a: casos con «ECG» y, más tarde, «CoronariografiaPrecoz» en menos de 2 horas
* **F41** [time] Limita los casos a: casos con «Coronariografia» y, más tarde, «UnidadCoronaria» con más de 1 hora entre ambos
* **F38** [mixed] Limita los casos a: casos con «PlantaCardiologia» y Edad mayor que 62
* **F39** [mixed] Limita los casos a: casos con «Muestra1h» y, más tarde, «CoronariografiaPrecoz», con DiagnosticoAlta igual a «IAMSEST»

### Circuits

* **C01** [act] Crea circuitos para esto: separa a los pacientes según cómo acaba su atención: alta, planta de cardiología o unidad coronaria
* **C02** [act] Crea circuitos para esto: clasifica a los pacientes según hayan ido a planta de cardiología, a la unidad coronaria o a angioplastia (el primero que cumpla)
* **C03** [act] Crea circuitos para esto: separa los pacientes que pasaron de la troponina de la primera hora a coronariografía precoz, los que solo tuvieron esa troponina y el resto
* **C04** [act] Crea circuitos para esto: separa los casos según cómo terminan: «Alta» o «PlantaCardiologia»
* **C05** [var] Crea circuitos para esto: separa a los pacientes por sexo
* **C06** [var] Crea un circuito por cada valor de AntecedenteCoronario: «no» o «si»
* **C07** [var] Crea circuitos para esto: separa los casos según Diabetes: «no» o «si»
* **C08** [var] Crea circuitos para esto: agrupa los casos por DiagnosticoAlta: «Dolor torácico no coronario» o «IAMSEST»
* **C09** [time] Crea tres circuitos: «ECG» y luego «PlantaCardiologia» en menos de 3 horas, el resto de los que tienen «ECG» y luego «PlantaCardiologia», y los demás
* **C10** [time] Haz tres circuitos según el tiempo entre «Cardiologia» y «PlantaCardiologia»: menos de 1 hora, más tiempo, y los que no tienen ese tramo
* **C11** [mixed] Crea tres circuitos: casos con AntecedenteCoronario igual a «no» que pasan por «CoronariografiaPrecoz», el resto de casos con AntecedenteCoronario igual a «no», y los demás
* **C12** [mixed] Haz tres circuitos: Diabetes igual a «no» y «Cardiologia», solo Diabetes igual a «no», y el resto

### Regions

* **R01** [act] Define una región desde «Muestra1h» hasta «CoronariografiaPrecoz»
* **R02** [act] Define una sección crítica desde «ECG» hasta «PlantaCardiologia»
* **R03** [act] Define una región desde «ECG» hasta «Cardiologia»
* **R04** [act] Define dos secciones críticas: de «CodigoInfarto» a «Coronariografia» y de «ICP» a «UnidadCoronaria»
* **R05** [act] Define dos regiones: de «ECG» a «Cardiologia» y de «ECG» a «PlantaCardiologia»
* **R06** [time] Define una región desde «ECG» hasta «PlantaCardiologia» cuando pasan menos de 3 horas entre ambos
* **R07** [time] Define una sección crítica desde «Cardiologia» hasta «PlantaCardiologia» cuando transcurren más de 2 horas
* **R08** [time] Define una sección crítica desde «ECG» hasta «Cardiologia» cuando transcurren más de 1 hora
* **R09** [mixed] Define una región desde «ECG» hasta «Cardiologia» en los casos con AntecedenteCoronario igual a «no»
* **R10** [mixed] Define una sección crítica desde «ECG» hasta «PlantaCardiologia» solo en los casos con Diabetes igual a «no»

### Milestones

* **M01** [act] Define «Alta» como hito
* **M02** [act] Marca «CodigoInfarto» y «CoronariografiaPrecoz» como hitos
* **M03** [act] Usa «ICP» como hito
* **M04** [act] Define «Coronariografia» como hito
* **M05** [act] Define «CoronariografiaPrecoz» y «CodigoInfarto» como hitos
* **M06** [act] Marca «CoronariografiaPrecoz», «ICP» y «PlantaCardiologia» como hitos
* **M07** [act] Define «Cardiologia» como hito
* **M08** [act] Marca «Cardiologia» y «UnidadCoronaria» como hitos
* **M09** [act] Define «Cardiologia», «CoronariografiaPrecoz» y «PlantaCardiologia» como hitos
* **M10** [act] Marca «PlantaCardiologia» y «CoronariografiaPrecoz» como hitos


## gestion_citas

### Filters

* **F01** [act] Limita los casos a: casos en los que ocurre «CitaPreferente»
* **F02** [act] Limita los casos a: pacientes/casos que pasaron por «RecordatorioCita»
* **F33** [var] Limita los casos a: casos con Edad mayor que 44
* **F34** [var] Limita los casos a: casos con Edad entre 44 y 61
* **F40** [time] Limita los casos a: casos con «TriajeAdministrativo» y, más tarde, «NoPresentado» en menos de 5 días
* **F41** [time] Limita los casos a: casos con «SolicitudCita» y, más tarde, «ConsultaTramite» con más de 7 días entre ambos
* **F39** [mixed] Limita los casos a: casos con «TriajeAdministrativo» y, más tarde, «SolicitudReconsulta», con CosteTramo igual a «medio»
* **F43** [mixed] Limita los casos a: casos con Edad mayor que 47 y «ConsultaTramite» en menos de 6 días desde «TriajeAdministrativo»

### Circuits

* **C01** [act] Crea circuitos para esto: separa las solicitudes según cómo terminan: resueltas, abandonadas, derivadas o con urgencias no urgentes
* **C02** [act] Crea circuitos para esto: clasifica a los pacientes según hayan tenido cita preferente, recordatorio o trámite resuelto en administración (el primero que cumpla)
* **C03** [act] Crea circuitos para esto: separa los pacientes que pasaron del triaje administrativo a una reconsulta, los que solo pasaron por el triaje y el resto
* **C04** [act] Crea circuitos para esto: separa los casos según cómo terminan: «CierreResuelto» o «AbandonoSolicitud»
* **C05** [var] Crea circuitos para esto: separa los casos según AccesoDigital: «si» o «no»
* **C06** [var] Crea circuitos para esto: agrupa los casos por BurnoutMedico: «bajo» o «alto»
* **C07** [var] Crea un circuito por cada valor de CosteTramo: «bajo» o «medio»
* **C08** [var] Crea circuitos para esto: separa los casos según Cupo: «CupoB» o «CupoA»
* **C09** [time] Crea tres circuitos: «NoPresentado» y luego «RecordatorioCita» en menos de 10 días, el resto de los que tienen «NoPresentado» y luego «RecordatorioCita», y los demás
* **C10** [time] Haz tres circuitos según el tiempo entre «ConsultaMedica» y «CierreResuelto»: menos de 14 minutoes, más tiempo, y los que no tienen ese tramo
* **C11** [mixed] Crea tres circuitos: casos con AccesoDigital igual a «si» que pasan por «TramiteResueltoAdministrativo», el resto de casos con AccesoDigital igual a «si», y los demás
* **C12** [mixed] Haz tres circuitos: BurnoutMedico igual a «bajo» y «DerivacionEspecialista», solo BurnoutMedico igual a «bajo», y el resto

### Regions

* **R01** [act] Define una región desde «TriajeAdministrativo» hasta «SolicitudReconsulta»
* **R02** [act] Define una sección crítica desde «CitaDemorada» hasta «ConsultaMedica»
* **R03** [act] Define una región desde «TriajeAdministrativo» hasta «ConsultaMedica»
* **R04** [act] Define dos secciones críticas: de «CitaProgramada» a «RecordatorioCita» y de «SolicitudCita» a «TramiteResueltoAdministrativo»
* **R05** [act] Define dos regiones: de «TriajeAdministrativo» a «ConsultaMedica» y de «SolicitudCita» a «AbandonoSolicitud»
* **R06** [time] Define una región desde «NoPresentado» hasta «RecordatorioCita» cuando pasan menos de 10 días entre ambos
* **R07** [time] Define una sección crítica desde «ConsultaMedica» hasta «CierreResuelto» cuando transcurren más de 14 minutoes
* **R08** [time] Define una región desde «CitaProgramada» hasta «NoPresentado» cuando pasan menos de 5 días entre ambos
* **R09** [mixed] Define una región desde «TriajeAdministrativo» hasta «ConsultaMedica» en los casos con AccesoDigital igual a «si»
* **R10** [mixed] Define una sección crítica desde «SolicitudCita» hasta «AbandonoSolicitud» solo en los casos con BurnoutMedico igual a «bajo»

### Milestones

* **M01** [act] Define «CierreResuelto» como hito
* **M02** [act] Marca «CitaProgramada» y «ConsultaTramite» como hitos
* **M03** [act] Usa «TramiteResueltoAdministrativo» como hito
* **M04** [act] Define «ConsultaResultados» como hito
* **M05** [act] Define «RecordatorioCita» y «ConsultaMedica» como hitos
* **M06** [act] Marca «PruebaComplementaria», «RecordatorioCita» y «AtencionUrgenciasNoUrgente» como hitos
* **M07** [act] Define «CitaDemorada» como hito
* **M08** [act] Marca «CierreDerivado» y «ConsultaTramite» como hitos
* **M09** [act] Define «ConsultaTramite», «CitaProgramada» y «DerivacionEspecialista» como hitos
* **M10** [act] Define «AbandonoSolicitud» como hito


## helpdesk

### Filters

* **F01** [act] Limit the cases to: cases in which 'Wait' occurs
* **F02** [act] Limit the cases to: cases in which 'Take in charge ticket' never occurs
* **F25** [time] Limit the cases to: cases with 'Wait' and, later, 'Resolve ticket' in less than 7 days
* **F26** [time] Limit the cases to: cases with 'Wait' and, later, 'Take in charge ticket' more than 6 days apart
* **F27** [time] Limit the cases to: cases with 'Assign seriousness' and, later, 'Wait' between 1 day and 14 days after
* **F03** [act] Limit the cases to: cases with 'Closed' and also 'Wait'
* **F04** [act] Limit the cases to: cases with 'Wait' but without 'Resolve SW anomaly'
* **F05** [act] Limit the cases to: cases with 'Schedule intervention' or 'Wait'

### Circuits

* **C01** [act] Create circuits for this: separate the tickets by how they end: closed, resolved, waiting or requiring an upgrade
* **C02** [act] Create circuits for this: separate the tickets taken in charge and later put on hold, those only taken in charge, and the rest
* **C03** [act] Create circuits for this: separate the cases with 'Wait' followed by 'Resolve ticket', those with only 'Wait' and the rest
* **C04** [act] Make three circuits: 'Wait' then 'Closed', only 'Wait', and all the others
* **C05** [time] Create three circuits: 'Assign seriousness' then 'Wait' in less than 5 days, the rest of those with 'Assign seriousness' then 'Wait', and all the others
* **C06** [time] Make three circuits by the time between 'Wait' and 'Resolve ticket': less than 7 days, longer, and those without that stretch
* **C07** [mixed] Create three circuits: cases with responsible_section equal to 'Value 1' that go through 'Wait', the rest of the cases with responsible_section equal to 'Value 1', and all the others
* **C08** [mixed] Make three circuits: support_section equal to 'Value 1' and 'Wait', only support_section equal to 'Value 1', and the rest
* **C09** [time] Create three circuits: 'Wait' then 'Closed' in less than 30 days, the rest of those with 'Wait' then 'Closed', and all the others
* **C10** [time] Make three circuits by the time between 'Resolve ticket' and 'Take in charge ticket': less than 1 day, longer, and those without that stretch
* **C11** [time] Create three circuits: 'Wait' then 'Take in charge ticket' in less than 4 days, the rest of those with 'Wait' then 'Take in charge ticket', and all the others
* **C12** [time] Make three circuits by the time between 'Take in charge ticket' and 'Wait': less than 20 hours, longer, and those without that stretch

### Regions

* **R01** [act] Define a region from 'Take in charge ticket' to 'Wait'
* **R02** [act] Define a critical section from 'Wait' to 'Resolve ticket'
* **R03** [act] Define a region from 'Resolve ticket' to 'Take in charge ticket'
* **R04** [act] Define two critical sections: from 'Wait' to 'Closed' and from 'Assign seriousness' to 'Wait'
* **R05** [act] Define two regions: from 'Resolve ticket' to 'Take in charge ticket' and from 'Wait' to 'Resolve ticket'
* **R06** [time] Define a region from 'Assign seriousness' to 'Wait' when less than 5 days pass between them
* **R07** [time] Define a critical section from 'Wait' to 'Resolve ticket' when more than 10 days elapse
* **R08** [time] Define a region from 'Wait' to 'Closed' when less than 30 days pass between them
* **R09** [mixed] Define a region from 'Resolve ticket' to 'Take in charge ticket' in the cases with responsible_section equal to 'Value 1'
* **R10** [mixed] Define a critical section from 'Wait' to 'Resolve ticket' only in the cases with support_section equal to 'Value 1'

### Milestones

* **M01** [act] Define 'Require upgrade' as a milestone
* **M02** [act] Mark 'Wait' and 'Insert ticket' as milestones
* **M03** [act] Define 'Require upgrade' and 'Insert ticket' as milestones
* **M04** [act] Mark 'Insert ticket', 'Wait' and 'Require upgrade' as milestones
* **M05** [act] Define 'Wait' and 'Require upgrade' as milestones
* **M06** [act] Mark 'Require upgrade', 'Wait' and 'Insert ticket' as milestones
* **M07** [act] Define 'Insert ticket' and 'Require upgrade' as milestones
* **M08** [act] Mark 'Require upgrade', 'Insert ticket' and 'Wait' as milestones
* **M09** [act] Define 'Insert ticket' and 'Wait' as milestones
* **M10** [act] Mark 'Require upgrade' and 'Wait' as milestones


## hospital_billing

### Filters

* **F01** [act] Limit the cases to: cases in which 'DELETE' occurs
* **F02** [act] Limit the cases to: cases that went through 'CHANGE DIAGN'
* **F33** [var] Limit the cases to: cases with speciality equal to 'L'
* **F34** [var] Limit the cases to: cases with speciality equal to 'K' or 'L'
* **F35** [time] Limit the cases to: cases with 'FIN' and, later, 'BILLED' in less than 14 days
* **F36** [time] Limit the cases to: cases with 'NEW' and, later, 'BILLED' more than 30 days apart
* **F37** [time] Limit the cases to: cases with 'NEW' and, later, 'CHANGE DIAGN' between 1 minute and 20 hours after
* **F03** [act] Limit the cases to: i want the cases where 'BILLED' appears

### Circuits

* **C01** [act] Create circuits for this: separate the billing packages by how they end: billed, just created, deleted or finalised
* **C02** [act] Create circuits for this: classify the packages by whether they were deleted, had a diagnosis change or were billed (first match wins)
* **C03** [act] Create circuits for this: separate the packages whose diagnosis was changed and that were later finalised, those with only the diagnosis change, and the rest
* **C04** [act] Create circuits for this: separate the cases by how they end: 'BILLED' or 'NEW'
* **C05** [var] Create one circuit for each value of speciality: 'K' or 'L'
* **C06** [time] Create three circuits: 'CHANGE DIAGN' then 'CODE OK' in less than 30 days, the rest of those with 'CHANGE DIAGN' then 'CODE OK', and all the others
* **C07** [time] Make three circuits by the time between 'RELEASE' and 'REOPEN': less than 14 days, longer, and those without that stretch
* **C08** [mixed] Create three circuits: cases with blocked equal to 'false' that go through 'CHANGE DIAGN', the rest of the cases with blocked equal to 'false', and all the others
* **C09** [time] Create three circuits: 'NEW' then 'CODE NOK' in less than 30 days, the rest of those with 'NEW' then 'CODE NOK', and all the others
* **C10** [time] Make three circuits by the time between 'NEW' and 'BILLED': less than 30 days, longer, and those without that stretch
* **C11** [time] Create three circuits: 'RELEASE' then 'CODE NOK' in less than 4 days, the rest of those with 'RELEASE' then 'CODE NOK', and all the others
* **C12** [time] Make three circuits by the time between 'RELEASE' and 'CODE OK': less than 4 days, longer, and those without that stretch

### Regions

* **R01** [act] Define a region from 'CHANGE DIAGN' to 'FIN'
* **R02** [act] Define a critical section from 'NEW' to 'BILLED'
* **R03** [act] Define a region from 'NEW' to 'REOPEN'
* **R04** [act] Define two critical sections: from 'RELEASE' to 'CODE OK' and from 'NEW' to 'DELETE'
* **R05** [act] Define two regions: from 'CHANGE DIAGN' to 'FIN' and from 'NEW' to 'REOPEN'
* **R06** [time] Define a region from 'CHANGE DIAGN' to 'CODE OK' when less than 30 days pass between them
* **R07** [time] Define a critical section from 'RELEASE' to 'REOPEN' when more than 20 days elapse
* **R08** [time] Define a region from 'NEW' to 'CODE NOK' when less than 30 days pass between them
* **R09** [mixed] Define a region from 'CHANGE DIAGN' to 'FIN' in the cases with blocked equal to 'false'
* **R10** [time] Define a critical section from 'NEW' to 'BILLED' when more than 30 days elapse

### Milestones

* **M01** [act] Define 'DELETE' as a milestone
* **M02** [act] Mark 'FIN' and 'BILLED' as milestones
* **M03** [act] Use 'CODE NOK' as a milestone
* **M04** [act] Define 'REOPEN' as a milestone
* **M05** [act] Define 'BILLED' and 'DELETE' as milestones
* **M06** [act] Mark 'REOPEN', 'CHANGE DIAGN' and 'DELETE' as milestones
* **M07** [act] Define 'RELEASE' as a milestone
* **M08** [act] Mark 'RELEASE' and 'CHANGE DIAGN' as milestones
* **M09** [act] Define 'CODE OK', 'DELETE' and 'CODE NOK' as milestones
* **M10** [act] Define 'STORNO' as a milestone


## ictus

### Filters

* **F01** [act] Limita los casos a: pacientes que recibieron trombólisis
* **F02** [act] Limita los casos a: pacientes que ingresaron en UCI
* **F33** [var] Limita los casos a: pacientes mayores de 80 años
* **F34** [var] Limita los casos a: pacientes con ictus grave
* **F42** [time] Limita los casos a: pacientes a los que se hizo el TAC en menos de 30 minutos desde el triaje
* **F43** [time] Limita los casos a: pacientes con más de una hora entre el triaje y la trombólisis
* **F40** [mixed] Limita los casos a: pacientes mayores de 70 años con trombólisis
* **F41** [mixed] Limita los casos a: mujeres con TAC y, más tarde, trombólisis

### Circuits

* **C01** [act] Crea circuitos para esto: separa los pacientes según cómo terminan: a casa, a un centro de rehabilitación, a una residencia o fallecidos
* **C02** [act] Crea circuitos para esto: divide a los pacientes en tres grupos: los que tuvieron trombectomía, los que solo tuvieron trombólisis y los que no recibieron reperfusión
* **C03** [act] Crea circuitos para esto: clasifica a los pacientes según dónde ingresan: UCI, unidad de ictus o planta
* **C04** [act] Crea circuitos para esto: separa los casos según cómo terminan: «AltaDomicilio» o «AltaResidencia»
* **C05** [var] Crea circuitos para esto: separa a los pacientes por gravedad al llegar: grave, moderada y leve
* **C06** [var] Crea circuitos para esto: separa los casos según AlimentacionSinCribado: «no» o «si»
* **C07** [var] Crea circuitos para esto: agrupa los casos por Anticoagulado: «no» o «si»
* **C08** [var] Crea un circuito por cada valor de CodigoActivado: «si» o «no»
* **C09** [time] Crea tres circuitos: «PlanAlta» y luego «AltaCentroRehabilitacion» en menos de 14 días, el resto de los que tienen «PlanAlta» y luego «AltaCentroRehabilitacion», y los demás
* **C10** [time] Haz tres circuitos según el tiempo entre «TAC» y «OrdenAlimentacionPrecoz»: menos de 4 horas, más tiempo, y los que no tienen ese tramo
* **C11** [mixed] Crea tres circuitos: casos con AlimentacionSinCribado igual a «no» que pasan por «DisfagiaDetectada», el resto de casos con AlimentacionSinCribado igual a «no», y los demás
* **C12** [mixed] Crea tres circuitos: casos con Anticoagulado igual a «no» que pasan por «Trombectomia», el resto de casos con Anticoagulado igual a «no», y los demás

### Regions

* **R01** [act] Define una región desde «TAC» hasta «Trombolisis»
* **R02** [act] Define una sección crítica desde «CribadoDisfagia» hasta «Logopedia»
* **R03** [act] Define una región desde «AngioTAC» hasta «ControlEvolutivo»
* **R04** [act] Define dos secciones críticas: de «TriajeUrgencias» a «TAC» y de «IngresoUnidadIctus» a «SalidaHospital»
* **R05** [act] Define dos regiones: de «AngioTAC» a «ControlEvolutivo» y de «IngresoUnidadIctus» a «CribadoDisfagia»
* **R06** [time] Define una región desde «PlanAlta» hasta «AltaCentroRehabilitacion» cuando pasan menos de 14 días entre ambos
* **R07** [time] Define una sección crítica desde «TAC» hasta «OrdenAlimentacionPrecoz» cuando transcurren más de 4 horas
* **R08** [time] Define una región desde «OrdenAlimentacionPrecoz» hasta «SalidaHospital» cuando pasan menos de 6 días entre ambos
* **R09** [mixed] Define una región desde «AngioTAC» hasta «ControlEvolutivo» en los casos con AlimentacionSinCribado igual a «no»
* **R10** [mixed] Define una sección crítica desde «IngresoUnidadIctus» hasta «CribadoDisfagia» solo en los casos con AnticoagulacionAlta igual a «si»

### Milestones

* **M01** [act] Define «Trombectomia» como hito
* **M02** [act] Marca «IngresoUnidadIctus» y «IngresoUCI» como hitos
* **M03** [act] Usa «TAC» como hito
* **M04** [act] Define «LlegadaPropiosMedios» como hito
* **M05** [act] Define «ActivacionCodigoIctus» y «AltaResidencia» como hitos
* **M06** [act] Marca «ActivacionCodigoIctus», «SalidaHospital» y «Rehabilitacion» como hitos
* **M07** [act] Define «OrdenAlimentacionPrecoz» como hito
* **M08** [act] Marca «SalidaHospital» y «Logopedia» como hitos
* **M09** [act] Define «ActivacionCodigoIctus», «TrasladoInterhospitalario» y «Llamada112» como hitos
* **M10** [act] Define «PlanAlta» como hito


## road_traffic_fine

### Filters

* **F01** [act] Limit the cases to: cases in which 'Send for Credit Collection' occurs
* **F02** [act] Limit the cases to: cases that went through 'Insert Fine Notification'
* **F33** [var] Limit the cases to: cases with lastSent equal to 'P'
* **F34** [var] Limit the cases to: cases with notificationType equal to 'P'
* **F38** [time] Limit the cases to: cases with 'Create Fine' and, later, 'Payment' in less than 7 days
* **F39** [time] Limit the cases to: cases with 'Insert Fine Notification' and, later, 'Add penalty' more than 30 days apart
* **F37** [mixed] Limit the cases to: cases with 'Add penalty' and, later, 'Payment', where notificationType is 'P'
* **F35** [var] Limit the cases to: cases with lastSent equal to 'P' and notificationType equal to 'P'

### Circuits

* **C01** [act] Create circuits for this: separate the fines by how they end: payment, credit collection, sent to the offender or appeal sent to the prefecture
* **C02** [act] Create circuits for this: classify the fines by whether they were sent for credit collection, had a notification registered or got a penalty (first match wins)
* **C03** [act] Create circuits for this: separate the fines that got a penalty and were then paid, those with only a penalty, and the rest
* **C04** [act] Create circuits for this: separate the cases by how they end: 'Payment' or 'Send for Credit Collection'
* **C05** [var] Create circuits for this: separate the fines by the kind of the last notice sent: P and N
* **C06** [var] Create circuits for this: split the cases in two groups by points: up to 0 and more than 0
* **C07** [time] Make three circuits by the time between 'Send Fine' and 'Insert Fine Notification': less than 14 days, longer, and those without that stretch
* **C08** [time] Create three circuits: 'Create Fine' then 'Send Fine' in less than 30 days, the rest of those with 'Create Fine' then 'Send Fine', and all the others
* **C09** [mixed] Make three circuits: notificationType equal to 'P' and 'Send for Credit Collection', only notificationType equal to 'P', and the rest
* **C10** [mixed] Create three circuits: cases with vehicleClass equal to 'A' that go through 'Add penalty', the rest of the cases with vehicleClass equal to 'A', and all the others
* **C11** [act] Create circuits for this: classify the cases by their last step: 'Payment', 'Send for Credit Collection' or 'Send Fine'
* **C12** [act] Create circuits for this: separate the cases by how they end: 'Send for Credit Collection' or 'Send Fine'

### Regions

* **R01** [act] Define a region from 'Add penalty' to 'Payment'
* **R02** [act] Define a critical section from 'Create Fine' to 'Send for Credit Collection'
* **R03** [act] Define a region from 'Create Fine' to 'Add penalty'
* **R04** [act] Define two critical sections: from 'Send Fine' to 'Insert Fine Notification' and from 'Create Fine' to 'Insert Fine Notification'
* **R05** [act] Define two regions: from 'Create Fine' to 'Add penalty' and from 'Create Fine' to 'Send for Credit Collection'
* **R06** [time] Define a critical section from 'Send Fine' to 'Insert Fine Notification' when more than 14 days elapse
* **R07** [time] Define a region from 'Create Fine' to 'Send Fine' when less than 30 days pass between them
* **R08** [time] Define a critical section from 'Create Fine' to 'Add penalty' when more than 30 days elapse
* **R09** [mixed] Define a region from 'Create Fine' to 'Add penalty' in the cases with lastSent equal to 'P'
* **R10** [mixed] Define a critical section from 'Create Fine' to 'Send for Credit Collection' only in the cases with notificationType equal to 'P'

### Milestones

* **M01** [act] Define 'Send for Credit Collection' as a milestone
* **M02** [act] Mark 'Send Fine' and 'Add penalty' as milestones
* **M03** [act] Use 'Payment' as a milestone
* **M04** [act] Define 'Insert Date Appeal to Prefecture' as a milestone
* **M05** [act] Define 'Insert Fine Notification' and 'Add penalty' as milestones
* **M06** [act] Mark 'Insert Fine Notification', 'Send Appeal to Prefecture' and 'Send for Credit Collection' as milestones
* **M07** [act] Define 'Payment' and 'Send Fine' as milestones
* **M08** [act] Mark 'Add penalty', 'Send Appeal to Prefecture' and 'Insert Fine Notification' as milestones
* **M09** [act] Define 'Insert Date Appeal to Prefecture' and 'Add penalty' as milestones
* **M10** [act] Mark 'Send for Credit Collection', 'Send Fine' and 'Insert Fine Notification' as milestones


## sepsis_cases

### Filters

* **F01** [act] Limit the cases to: cases in which 'Return ER' occurs
* **F02** [act] Limit the cases to: cases that went through 'Release A'
* **F33** [var] Limit the cases to: cases with Age greater than 65
* **F34** [var] Limit the cases to: cases with Age between 65 and 80
* **F40** [time] Limit the cases to: cases with 'IV Liquid' and, later, 'Release A' in less than 5 days
* **F41** [time] Limit the cases to: cases with 'IV Antibiotics' and, later, 'Release A' more than 6 days apart
* **F38** [mixed] Limit the cases to: cases with 'Return ER' and Age greater than 75
* **F39** [mixed] Limit the cases to: cases with 'IV Antibiotics' and, later, 'LacticAcid', where DiagnosticUrinarySediment is 'true'

### Circuits

* **C01** [act] Create circuits for this: separate the patients by how their pathway ends: release A, return to the ER, IV antibiotics or release B
* **C02** [act] Create circuits for this: classify the patients by whether they returned to the ER, were released with type A or were admitted to the ICU (first match wins)
* **C03** [act] Create circuits for this: separate the patients who got antibiotics followed by a lactic acid measurement, those with only antibiotics, and the rest
* **C04** [act] Create circuits for this: separate the cases by how they end: 'Release A' or 'Return ER'
* **C05** [var] Create circuits for this: separate the cases by DiagnosticArtAstrup: 'false' or 'true'
* **C06** [var] Create circuits for this: group the cases by DiagnosticBlood: 'true' or 'false'
* **C07** [var] Create one circuit for each value of DiagnosticECG: 'true' or 'false'
* **C08** [var] Create circuits for this: separate the cases by DiagnosticIC: 'true' or 'false'
* **C09** [time] Create three circuits: 'IV Antibiotics' then 'IV Liquid' in less than 14 minutes, the rest of those with 'IV Antibiotics' then 'IV Liquid', and all the others
* **C10** [time] Make three circuits by the time between 'Admission IC' and 'CRP': less than 3 hours, longer, and those without that stretch
* **C11** [mixed] Create three circuits: cases with DiagnosticArtAstrup equal to 'false' that go through 'Return ER', the rest of the cases with DiagnosticArtAstrup equal to 'false', and all the others
* **C12** [mixed] Make three circuits: DiagnosticBlood equal to 'true' and 'IV Antibiotics', only DiagnosticBlood equal to 'true', and the rest

### Regions

* **R01** [act] Define a region from 'IV Antibiotics' to 'LacticAcid'
* **R02** [act] Define a critical section from 'Leucocytes' to 'ER Sepsis Triage'
* **R03** [act] Define a region from 'CRP' to 'Leucocytes'
* **R04** [act] Define two critical sections: from 'Admission IC' to 'CRP' and from 'ER Triage' to 'Release A'
* **R05** [act] Define two regions: from 'CRP' to 'Leucocytes' and from 'LacticAcid' to 'ER Sepsis Triage'
* **R06** [time] Define a region from 'IV Antibiotics' to 'IV Liquid' when less than 14 minutes pass between them
* **R07** [time] Define a critical section from 'Admission IC' to 'CRP' when more than 7 hours elapse
* **R08** [time] Define a region from 'CRP' to 'Admission IC' when less than 2 hours pass between them
* **R09** [mixed] Define a region from 'CRP' to 'Leucocytes' in the cases with DiagnosticArtAstrup equal to 'false'
* **R10** [mixed] Define a critical section from 'LacticAcid' to 'ER Sepsis Triage' only in the cases with DiagnosticBlood equal to 'true'

### Milestones

* **M01** [act] Define 'Release B' as a milestone
* **M02** [act] Mark 'LacticAcid' and 'Admission NC' as milestones
* **M03** [act] Use 'Admission IC' as a milestone
* **M04** [act] Define 'Release A' as a milestone
* **M05** [act] Define 'Return ER' and 'LacticAcid' as milestones
* **M06** [act] Mark 'IV Liquid', 'Return ER' and 'LacticAcid' as milestones
* **M07** [act] Define 'Release D' as a milestone
* **M08** [act] Mark 'Release D' and 'IV Liquid' as milestones
* **M09** [act] Define 'Return ER', 'Admission NC' and 'Release D' as milestones
* **M10** [act] Define 'Release C' as a milestone


## sepsis_hospital_complejo

### Filters

* **F01** [act] Limita los casos a: casos en los que ocurre «ControlFoco»
* **F02** [act] Limita los casos a: pacientes/casos que pasaron por «Reingreso30d»
* **F32** [var] Limita los casos a: casos con Edad mayor que 61
* **F33** [var] Limita los casos a: casos con Edad entre 61 y 76
* **F39** [time] Limita los casos a: casos con «Liquidos» y, más tarde, «ValoracionInicialPlanta» en menos de 10 horas
* **F40** [time] Limita los casos a: casos con «AlertaSepsis» y, más tarde, «OxigenoTerapia» con más de 4 horas entre ambos
* **F37** [mixed] Limita los casos a: casos con «ControlFoco» y Edad mayor que 68
* **F38** [mixed] Limita los casos a: casos con «Reevaluacion» y, más tarde, «ReevaluacionPostVasopresores», con IngresoHospital igual a «si»

### Circuits

* **C01** [act] Crea circuitos para esto: separa a los pacientes según cómo terminan: fin de seguimiento, reingreso, fallecimiento en UCI o fallecimiento en planta
* **C02** [act] Crea circuitos para esto: clasifica a los pacientes según hayan tenido control del foco, reingreso o valoración inicial en planta (el primero que cumpla)
* **C03** [act] Crea circuitos para esto: separa los pacientes con una reevaluación seguida de reevaluación tras vasopresores, los que solo se reevaluaron y el resto
* **C04** [act] Crea circuitos para esto: separa los casos según cómo terminan: «FinSeguimiento» o «Reingreso30d»
* **C05** [var] Crea circuitos para esto: agrupa los casos por AntibioticoPrevio: «no» o «si»
* **C06** [var] Crea un circuito por cada valor de CodigoSepsis: «no» o «si»
* **C07** [var] Crea circuitos para esto: separa los casos según ControlFoco: «no» o «si»
* **C08** [var] Crea circuitos para esto: agrupa los casos por Destino: «Alta» o «Reingreso30d»
* **C09** [time] Crea tres circuitos: «AnaliticaObservacion» y luego «ReevaluacionPostVasopresores» en menos de 10 horas, el resto de los que tienen «AnaliticaObservacion» y luego «ReevaluacionPostVasopresores», y los demás
* **C10** [time] Haz tres circuitos según el tiempo entre «AnaliticaObservacion» y «Liquidos»: menos de 1 hora, más tiempo, y los que no tienen ese tramo
* **C11** [mixed] Crea tres circuitos: casos con AntibioticoPrevio igual a «no» que pasan por «ReevaluacionPostControl», el resto de casos con AntibioticoPrevio igual a «no», y los demás
* **C12** [mixed] Haz tres circuitos: CodigoSepsis igual a «no» y «SoporteRenal», solo CodigoSepsis igual a «no», y el resto

### Regions

* **R01** [act] Define una región desde «Reevaluacion» hasta «ReevaluacionPostVasopresores»
* **R02** [act] Define una sección crítica desde «AnaliticaObservacion» hasta «ControlFoco»
* **R03** [act] Define una región desde «IngresoPlanta» hasta «ValoracionInicialPlanta»
* **R04** [act] Define dos secciones críticas: de «ImagenFoco» a «ValoracionInicialPlanta» y de «AlertaSepsis» a «AltaUrgencias»
* **R05** [act] Define dos regiones: de «IngresoPlanta» a «ValoracionInicialPlanta» y de «SoporteRenal» a «ValoracionDiariaPlanta»
* **R06** [time] Define una región desde «AnaliticaObservacion» hasta «ReevaluacionPostVasopresores» cuando pasan menos de 10 horas entre ambos
* **R07** [time] Define una sección crítica desde «AnaliticaObservacion» hasta «Liquidos» cuando transcurren más de 1 hora
* **R08** [time] Define una sección crítica desde «ControlFoco» hasta «DescensoAPlanta» cuando transcurren más de 4 días
* **R09** [mixed] Define una región desde «IngresoPlanta» hasta «ValoracionInicialPlanta» en los casos con AntibioticoPrevio igual a «no»
* **R10** [mixed] Define una región desde «Triaje» hasta «IngresoPlanta» en los casos con ControlFoco igual a «no»

### Milestones

* **M01** [act] Define «FinSeguimiento» como hito
* **M02** [act] Marca «ExitusUCI» y «ValoracionInicialPlanta» como hitos
* **M03** [act] Usa «AlertaSepsis» como hito
* **M04** [act] Define «DeterioroPlanta» como hito
* **M05** [act] Define «Antibiotico» y «ValoracionInicialPlanta» como hitos
* **M06** [act] Marca «ReevaluacionPostControl», «ImagenFoco» y «ValoracionDiariaPlanta» como hitos
* **M07** [act] Define «ReevaluacionPostVasopresores» como hito
* **M08** [act] Marca «ExitusPlanta» y «IngresoUCI» como hitos
* **M09** [act] Define «DeterioroPlanta», «ExitusPlanta» y «ControlFoco» como hitos
* **M10** [act] Define «AltaPlanta» como hito
